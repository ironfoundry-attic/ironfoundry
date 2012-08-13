namespace IronFoundry.Bosh.IoC
{
    using IronFoundry.Bosh.Agent;
    using IronFoundry.Bosh.Agent.Handlers;
    using IronFoundry.Misc.Agent;
    using StructureMap.Configuration.DSL;

    public class BoshRegistry : Registry
    {
        public BoshRegistry()
        {
            For<IAgent>().Use<BoshAgent>();

            For<IMessageHandler>().AddInstances(x =>
                {
                    x.Type<Noop>().Named("noop");
                    x.Type<Ping>().Named("ping");
                    x.Type<Shutdown>().Named("shutdown");
                    x.Type<GetTask>().Named("get_task");
                }
            );
        }
    }
}