namespace IronFoundry.Bosh.IoC
{
    using IronFoundry.Bosh.Agent;
    using IronFoundry.Misc.Agent;
    using StructureMap.Configuration.DSL;

    public class BoshRegistry : Registry
    {
        public BoshRegistry()
        {
            For<IAgent>().Use<BoshAgent>();
        }
    }
}