namespace IronFoundry.Dea.IoC
{
    using IronFoundry.Dea.Agent;
    using IronFoundry.Dea.Configuration;
    using IronFoundry.Dea.Providers;
    using IronFoundry.Misc.Agent;
    using StructureMap.Configuration.DSL;

    public class DeaRegistry : Registry
    {
        public DeaRegistry()
        {
            For<IAgent>().Use<DeaAgent>();
            For<IDeaConfig>().Singleton().Use<DeaConfig>();
            For<IHealthzProvider>().Singleton().Use<HealthzProvider>();
            For<IVarzProvider>().Singleton().Use<VarzProvider>();
        }
    }
}