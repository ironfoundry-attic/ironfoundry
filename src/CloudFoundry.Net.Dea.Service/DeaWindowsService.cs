namespace CloudFoundry.Net.Dea.Service
{
    using System;
    using System.ServiceProcess;
    using System.Threading;
    using System.Threading.Tasks;
    using NLog;

    [System.ComponentModel.DesignerCategory(@"Code")]
    partial class DeaWindowsService : ServiceBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan INITIAL_INTERVAL = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan DEFAULT_INTERVAL = TimeSpan.FromSeconds(5);

        private readonly Agent agent;
        private readonly Task agentTask;
        private readonly Timer agentMonitorTimer;

        public DeaWindowsService()
        {
            CanPauseAndContinue = false;
            initializeEventLog();
            ServiceName = "DeaWindowsService";
            agent = new Agent();
            agentTask = new Task(() => agent.Start());
            agentMonitorTimer = new Timer(agentMonitor);
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
            agentTask.Start();
            agentMonitorTimer.Change(INITIAL_INTERVAL, DEFAULT_INTERVAL);
        }

        protected override void OnStop()
        {
            agent.Stop();
            agentTask.Wait(TimeSpan.FromMinutes(2));
        }

        private void agentMonitor(object argState)
        {
            Timer t = (Timer)argState;
            t.Stop();
            if (agent.Error)
            {
                OnStop();
#if DEBUG
                if (Environment.UserInteractive)
                {
                    Console.WriteLine("Agent stopped due to error.");
                    return;
                }
                else
#endif
                {
                    base.Stop();
                    return;
                }
            }

            t.Restart(DEFAULT_INTERVAL);
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