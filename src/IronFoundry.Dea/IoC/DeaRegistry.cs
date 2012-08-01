namespace IronFoundry.Dea.IoC
{
    using IronFoundry.Dea.Providers;
    using StructureMap.Configuration.DSL;
    using IronFoundry.Dea.Configuration;

    public class DeaRegistry : Registry
    {
        public DeaRegistry()
        {
            For<IDeaConfig>().Singleton().Use<DeaConfig>();
            For<IHealthzProvider>().Singleton().Use<HealthzProvider>();
            For<IVarzProvider>().Singleton().Use<VarzProvider>();
        }
    }
}