namespace IronFoundry.Warden.ProcessIsolationService
{
    using System;
    using System.ServiceModel;
    using System.ServiceProcess;
    using NLog;
    using ProcessIsolation;
    using ProcessIsolation.Service;

    [System.ComponentModel.DesignerCategory(@"Code")]
    public class SelfHostedProcessHostService : ServiceBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private ServiceHost serviceHost;

        public void StartService(string[] args)
        {
            OnStart(args);
        }

        public void StopService()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                log.Info("Starting service...");

                var binding = IpcEndpointConfig.Binding();
                var address = IpcEndpointConfig.ServiceAddress(args.Length > 0 ? args[0] : null);

                var instance = new ProcessHostService();
                serviceHost = new ServiceHost(instance);
                serviceHost.AddServiceEndpoint(typeof(IProcessHostService), binding, address);
                serviceHost.Open();
            }
            catch (Exception ex)
            {
                log.ErrorException("Unable to start services", ex);
            }
        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                log.Info("Closing self host...");

                try
                {
                    serviceHost.Close();
                    log.Info("Service host was closed.");
                }
                catch (Exception ex)
                {
                    log.Error("Failed to close service host, aborting communications.", ex);
                    serviceHost.Abort();
                }
            }
        }
    }
}
