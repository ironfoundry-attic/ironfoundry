namespace IronFoundry.Warden.ProcessIsolationService
{
    using System;
    using System.ServiceModel;
    using System.ServiceProcess;
    using NLog;
    using ProcessIsolation.Service;

    [System.ComponentModel.DesignerCategory(@"Code")]
    public class SelfHostJobObjectService : ServiceBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private ServiceHost serviceHost;

        public void StartService()
        {
            OnStart(null);
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
                var instance = new JobObjectService();
                serviceHost = new ServiceHost(instance);
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
