namespace IronFoundry.Misc.WinService
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Misc.Agent;
    using IronFoundry.Misc.Logging;

    [System.ComponentModel.DesignerCategory(@"Code")]
    public class AgentWinService : IService
    {
        private static readonly TimeSpan ThirtySecondInterval = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan FiveSecondInterval = TimeSpan.FromSeconds(5);

        private readonly ILog log;
        private readonly IAgent agent;
        private readonly Task agentTask;
        private readonly Timer agentMonitorTimer;

        public AgentWinService(ILog log, IAgent agent)
        {
            this.log = log;
            this.agent = agent;

            agentTask = new Task(() => agent.Start());
            agentMonitorTimer = new Timer(agentMonitor);
        }

        public string ServiceName
        {
            get { return "IronFoundry." + agent.Name + ".Service"; }
        }

        public ushort StartIndex
        {
            get { return 10; }
        }

        public StartServiceResult StartService(IntPtr serviceHandle)
        {
            agentTask.Start();
            agentMonitorTimer.Change(ThirtySecondInterval, FiveSecondInterval);
            return new StartServiceResult();
        }

        public void StopService()
        {
            agentMonitorTimer.Stop();
            agent.Stop();
            agentTask.Wait(ThirtySecondInterval);
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
            t.Restart(FiveSecondInterval);
        }
    }
}