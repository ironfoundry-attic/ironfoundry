namespace CloudFoundry.Net.Dea.Service
{
    using System;
    using System.ServiceProcess;
    using NLog;

    [System.ComponentModel.DesignerCategory(@"Code")]
    partial class DeaWindowsService : ServiceBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly Agent agent;

        public DeaWindowsService()
        {
            CanPauseAndContinue = false;
            initializeEventLog();
            ServiceName = "DeaWindowsService";
            agent = new Agent();
        }

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
            agent.Start();
        }

        protected override void OnStop()
        {
            agent.Stop();
        }

        private void initializeEventLog()
        {
            try
            {
                AutoLog = false;
                EventLogger.Info("Init logging.");
            }
            catch (Exception ex)
            {
                logger.ErrorException("Unable to setup event log.", ex);
                AutoLog = true;
            }
        }
    }
}