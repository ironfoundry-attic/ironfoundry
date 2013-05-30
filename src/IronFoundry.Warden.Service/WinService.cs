namespace IronFoundry.Warden.Service
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Server;
    using NLog;
    using Topshelf;

    public class WinService : ServiceControl
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private readonly IContainerManager containerManager = new ContainerManager();
        private readonly TcpServer wardenServer;
        private readonly Task wardenServerTask;

        public WinService()
        {
            this.wardenServer = new TcpServer(this.containerManager, cts.Token);
            this.wardenServerTask = new Task(wardenServer.RunServer, cts.Token);
        }

        public bool Start(HostControl hostControl)
        {
            wardenServerTask.Start();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            try
            {
                cts.Cancel();
                Task.WaitAll(new[] { wardenServerTask }, (int)TimeSpan.FromSeconds(25).TotalMilliseconds);
                containerManager.Dispose();
            }
            catch (Exception ex)
            {
                log.InfoException(ex.Message, ex);
            }
            return true;
        }
    }
}
