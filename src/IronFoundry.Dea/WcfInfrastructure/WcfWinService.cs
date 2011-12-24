namespace IronFoundry.Dea.WcfInfrastructure
{
    using System;
    using System.ServiceModel;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Properties;
    using IronFoundry.Dea.WinService;

    public abstract class WcfWinService : IService
    {
        private readonly ILog log;
        private readonly bool shouldRun = false;

        protected ServiceHost serviceHost;

        public WcfWinService(ILog log, bool shouldRun)
            : this(log, shouldRun, null) { }

        public WcfWinService(ILog log, bool shouldRun, ServiceHost serviceHost)
        {
            this.log = log;
            this.shouldRun = shouldRun;
            this.serviceHost = serviceHost;
        }

        public abstract ushort StartIndex { get; }

        public string ServiceName
        {
            get { return serviceHost.Description.ServiceType.FullName; }
        }

        public virtual StartServiceResult StartService(IntPtr ignored)
        {
            if (shouldRun)
            {
                log.Debug(Resources.WcfService_StartingHost_Fmt, ServiceName);
                serviceHost.Open();
            }
            else
            {
                log.Info(Resources.WcfService_NotConfiguredToRun_Fmt, ServiceName);
            }
            return new StartServiceResult();
        }

        public virtual void StopService()
        {
            if (shouldRun && null != serviceHost)
            {
                log.Debug(Resources.WcfService_StoppingHost_Fmt, ServiceName);

                var disposableService = serviceHost as IDisposable;
                if (null != disposableService)
                {
                    disposableService.Dispose();
                }

                try
                {
                    if (serviceHost.State == CommunicationState.Faulted)
                    {
                        serviceHost.Abort();
                    }
                    else
                    {
                        serviceHost.Close();
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex, Resources.WcfService_ErrorClosingHost_Fmt, ex.Message);
                    serviceHost.Abort();
                }
            }
        }
    }
}