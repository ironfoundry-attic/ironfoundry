namespace IronFoundry.Dea.WinService
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Dea.Agent;
    using IronFoundry.Dea.Logging;

    [System.ComponentModel.DesignerCategory(@"Code")]
    public class DeaWinService : IService
    {
        private static readonly TimeSpan INITIAL_INTERVAL = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan DEFAULT_INTERVAL = TimeSpan.FromSeconds(5);

        private readonly ILog log;
        private readonly IAgent agent;
        private readonly Task agentTask;
        private readonly Timer agentMonitorTimer;

        public DeaWinService(ILog log, IAgent agent)
        {
            this.log = log;
            this.agent = agent;

            agentTask = new Task(() => agent.Start());
            agentMonitorTimer = new Timer(agentMonitor);
        }

        public string ServiceName
        {
            get { return "IronFoundry.Dea.Service"; }
        }

        public ushort StartIndex
        {
            get { return 10; }
        }

        public StartServiceResult StartService(IntPtr serviceHandle)
        {
            agentTask.Start();
            agentMonitorTimer.Change(INITIAL_INTERVAL, DEFAULT_INTERVAL);
            return new StartServiceResult();
        }

        public void StopService()
        {
            agentMonitorTimer.Stop();
            agent.Stop();
            agentTask.Wait(TimeSpan.FromMinutes(30));
        }

        private void agentMonitor(object argState)
        {
            Timer t = (Timer)argState;
            t.Stop();
            if (agent.Error)
            {
                StopService();
#if DEBUG
                if (Environment.UserInteractive)
                {
                    Console.WriteLine("Agent stopped due to error.");
                    return;
                }
                else
#endif
                {
                    return;
                }
            }
            t.Restart(DEFAULT_INTERVAL);
        }
    }
}