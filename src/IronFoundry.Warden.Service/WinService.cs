namespace IronFoundry.Warden.Service
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Jobs;
    using IronFoundry.Warden.Server;
    using IronFoundry.Warden.Utilities;
    using NLog;
    using Topshelf;

    public class WinService : ServiceControl
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly IContainerManager containerManager;
        private readonly TcpServer wardenServer;
        private readonly Task wardenServerTask;

        public WinService()
        {
            this.cancellationTokenSource = Statics.CancellationTokenSource;
            this.containerManager = Statics.ContainerManager;
            this.wardenServer = new TcpServer(containerManager, Statics.JobManager, cancellationTokenSource.Token);
            this.wardenServerTask = new Task(wardenServer.RunServer, cancellationTokenSource.Token);
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
                cancellationTokenSource.Cancel();
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
