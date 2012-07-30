namespace IronFoundry.Dea.Services
{
    using System.ServiceModel.Channels;
    using IronFoundry.Dea.Providers;
    using IronFoundry.Misc.Logging;
    
    public class MonitoringService : IMonitoringService
    {
        private readonly ILog log;
        private readonly IWebOperationContextProvider webContext;
        private readonly IHealthzProvider healthzProvider;
        private readonly IVarzProvider varzProvider;

        public MonitoringService(ILog log, IWebOperationContextProvider webContext,
            IHealthzProvider healthzProvider, IVarzProvider varzProvider)
        {
            this.log             = log;
            this.webContext      = webContext;
            this.healthzProvider = healthzProvider;
            this.varzProvider    = varzProvider;
        }

        public Message GetHealthz()
        {
            string message = healthzProvider.GetHealthz();
            return webContext.CreateTextResponse(message, "text/plaintext");
        }

        public Message GetVarz()
        {
            string message = varzProvider.GetVarzJson();
            return webContext.CreateTextResponse(message, "application/json");
        }
    }
}