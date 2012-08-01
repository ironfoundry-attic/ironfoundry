namespace IronFoundry.Nats.IoC
{
    using IronFoundry.Nats.Configuration;
    using StructureMap.Configuration.DSL;

    public class NatsRegistry : Registry
    {
        public NatsRegistry()
        {
            For<INatsConfig>().Singleton().Use<NatsConfig>();
        }
    }
}