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

    public class JobObjectClient : IJobObjectClient, IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly Mutex serviceMutex = new Mutex();

        protected virtual IJobObjectService ServiceInstance { get; set; }
        protected virtual ICommunicationObject ServiceChannel { get; set; }
        protected virtual ChannelFactory<IJobObjectService> Factory { get; set; }
        protected virtual InstanceContext Context { get; set; }

        private JobObjectServiceClientCallback callback;
        private readonly string configurationName;

        public JobObjectClient(string configurationName)
        {
            this.configurationName = configurationName;
        }

        private static int GetCurrentProcessID()
        {
            return Process.GetCurrentProcess().Id;
        }

        #region IJobObjectClient implementation
        public event EventHandler<EventArgs<string>> OutputReceived;
        public event EventHandler<EventArgs<string>> ErrorReceived;
        public event EventHandler<EventArgs<int>> ProcessExited;
        public event EventHandler<EventArgs<string>> ServiceMessageReceived;

        public void Register()
        {
            RegisterJobClient(GetCurrentProcessID());
        }

        public void Unregister()
        {
            UnregisterJobClient(GetCurrentProcessID());
        }
        #endregion

        #region IJobObjectService implementation (Service Proxy)
        public void RegisterJobClient(int processID)
        {
            log.Trace("Client Call: RegisterJobClient({0})", processID);
            GetChannel().RegisterJobClient(processID);
        }

        public void UnregisterJobClient(int processID)
        {
            log.Trace("Client Call: UnregisterJobClient({0})", processID);
            GetChannel().UnregisterJobClient(processID);
        }

        public void SetJobLimits(JobObjectLimits limits)
        {
            log.Trace("Client Call: SetJobLimits");
            GetChannel().SetJobLimits(limits);
        }

        public void StartProcess(string fileName, string workingDirectory, string args)
        {
            log.Trace("Client Call: StartProcess");
            GetChannel().StartProcess(fileName, workingDirectory, args);
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
        private IJobObjectService GetChannel()
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
                callback = new JobObjectServiceClientCallback();
                callback.OnError += ErrorReceived;
                callback.OnOutput += OutputReceived;
                callback.OnProcessExited += ProcessExited;
                callback.OnServiceMessage += ServiceMessageReceived;

                Context = new InstanceContext(callback);
                HookFaultEvents(Context);

                Factory = new DuplexChannelFactory<IJobObjectService>(Context, configurationName);
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

                CleanupCommunicationObject(ServiceChannel);
                ServiceChannel = null;
                ServiceInstance = null;

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
