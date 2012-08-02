namespace IronFoundry.Dea.Providers
{
    using IronFoundry.Dea.Properties;
    using IronFoundry.Misc.Logging;

    public class HealthzProvider : IHealthzProvider
    {
        const string HealtzOK = "ok\n";

        private readonly ILog log;

        public HealthzProvider(ILog log)
        {
            this.log = log;
        }

        public string GetHealthz()
        {
            log.Debug(Resources.HealthzProvider_GetHealthzCalled_Message);
            return HealtzOK;
        }
    }
}