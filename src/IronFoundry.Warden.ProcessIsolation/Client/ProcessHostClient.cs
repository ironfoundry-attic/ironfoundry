namespace IronFoundry.Warden.ProcessIsolation.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;
    using NLog;
    using Service;

    public class ProcessHostClient : IProcessHostClient, IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly Mutex serviceMutex = new Mutex();

        protected virtual IProcessHostService ServiceInstance { get; set; }
        protected virtual ICommunicationObject ServiceChannel { get; set; }
        protected virtual ChannelFactory<IProcessHostService> Factory { get; set; }
        protected virtual InstanceContext Context { get; set; }

        private ProcessHostClientCallback callback;
        private readonly string uniquePipeName;
        
        public ProcessHostClient(string uniquePipeName)
        {
            if (uniquePipeName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("uniquePipeName");
            }
            this.uniquePipeName = uniquePipeName;
        }

        private static int GetCurrentProcessID()
        {
            return Process.GetCurrentProcess().Id;
        }

        #region IProcessHostClient implementation
        public event EventHandler<ProcessEventArgs<string>> OutputReceived;
        public event EventHandler<ProcessEventArgs<string>> ErrorReceived;
        public event EventHandler<ProcessEventArgs<int>> ProcessExited;
        public event EventHandler<EventArgs<string>> ServiceMessageReceived;

        public void Register()
        {
            RegisterClient(GetCurrentProcessID());
        }

        public void Unregister()
        {
            UnregisterClient(GetCurrentProcessID());
        }
        #endregion

        #region IProcessHostService implementation (Service Proxy)
        public void RegisterClient(int processID)
        {
            log.Trace("Client Call: RegisterClient({0})", processID);
            GetChannel().RegisterClient(processID);
        }

        public void UnregisterClient(int processID)
        {
            log.Trace("Client Call: UnregisterClient({0})", processID);
            GetChannel().UnregisterClient(processID);
        }

        public void SetProcessLimits(int processID, ResourceLimits limits)
        {
            log.Trace("Client Call: SetProcessLimits({0})", processID);
            GetChannel().SetProcessLimits(processID, limits);
        }

        public void SetJobLimits(ResourceLimits limits)
        {
            log.Trace("Client Call: SetJobLimits");
            GetChannel().SetJobLimits(limits);
        }

        public int StartProcess(string fileName, string workingDirectory, string args)
        {
            log.Trace("Client Call: StartProcess");
            return GetChannel().StartProcess(fileName, workingDirectory, args);
        }

        public void StopProcess(int processID)
        {
            log.Trace("Client Call: StopProcess({0})", processID);
            GetChannel().StopProcess(processID);
        }

        public List<ProcessInfo> ListProcesses()
        {
            log.Trace("Client Call: ListProcesses");
            return GetChannel().ListProcesses();
        }

        #endregion

        #region Channel Management
        private IProcessHostService GetChannel()
        {
            serviceMutex.WaitOne();

            try
            {
                if (ServiceInstance == null)
                {
                    ReconnectServiceChannel();
                }

                if (ServiceInstance == null)
                {
                    throw new ApplicationException("Unable to connect to job object service.");
                }
            }
            finally
            {
                serviceMutex.ReleaseMutex();
            }
            return ServiceInstance;
        }

        private void ReconnectServiceChannel()
        {
            serviceMutex.WaitOne();

            try
            {
                callback = new ProcessHostClientCallback();
                callback.OnError += ErrorReceived;
                callback.OnOutput += OutputReceived;
                callback.OnProcessExited += ProcessExited;
                callback.OnServiceMessage += ServiceMessageReceived;

                Context = new InstanceContext(callback);
                HookFaultEvents(Context);

                var binding = IpcEndpointConfig.Binding();
                var endpoint = IpcEndpointConfig.ClientAddress(uniquePipeName);

                Factory = new DuplexChannelFactory<IProcessHostService>(Context, binding, endpoint);
                HookFaultEvents(Factory);

                ServiceInstance = Factory.CreateChannel();

                ServiceChannel = ServiceInstance as ICommunicationObject;
                if (ServiceChannel != null)
                {
                    HookFaultEvents(ServiceChannel);
                    ServiceChannel.Open();
                }
            }
            finally
            {
                serviceMutex.ReleaseMutex();
            }
        }

        private void CleanupServiceChannel()
        {
            serviceMutex.WaitOne();

            try
            {
                if (callback != null)
                {
                    callback.OnError -= ErrorReceived;
                    callback.OnOutput -= OutputReceived;
                    callback.OnProcessExited -= ProcessExited;
                    callback.OnServiceMessage -= ServiceMessageReceived;
                    callback = null;
                }

                CleanupCommunicationObject(Context);
                Context = null;

                ServiceInstance = null;
                CleanupCommunicationObject(ServiceChannel);
                ServiceChannel = null;

                CleanupCommunicationObject(Factory);
                Factory = null;
            }
            finally
            {
                serviceMutex.ReleaseMutex();
            }
        }

        private void HookFaultEvents(ICommunicationObject communicationObject)
        {
            communicationObject.Closing += CommunicationObject_Closing;
            communicationObject.Faulted += CommunicationObject_Faulted;
        }

        private void CleanupCommunicationObject(ICommunicationObject communicationObject)
        {
            if (communicationObject != null)
            {
                communicationObject.Faulted -= CommunicationObject_Faulted;
                communicationObject.Closing -= CommunicationObject_Closing;

                if (communicationObject.State == CommunicationState.Faulted)
                {
                    communicationObject.Abort();
                }
                else
                {
                    communicationObject.Close();
                }
            }
        }

        private void CommunicationObject_Closing(object sender, EventArgs e)
        {
            // don't block as this could be initiated from service side
            Task.Run(new Action(CleanupServiceChannel));
        }

        private void CommunicationObject_Faulted(object sender, EventArgs e)
        {
            // don't block as this could be initiated from service side
            Task.Run(new Action(CleanupServiceChannel));
        }
        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            Unregister();
            CleanupServiceChannel();
        }

        #endregion
    }
}
