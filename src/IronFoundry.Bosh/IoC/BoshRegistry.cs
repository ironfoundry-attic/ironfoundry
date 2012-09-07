namespace IronFoundry.Bosh.IoC
{
    using IronFoundry.Bosh.Agent;
    using IronFoundry.Bosh.Agent.Handlers;
    using IronFoundry.Bosh.Configuration;
    using IronFoundry.Misc.Agent;
    using StructureMap.Configuration.DSL;

    public class BoshRegistry : Registry
    {
        public BoshRegistry()
        {
            For<IBoshConfig>().Singleton().Use<BoshConfig>();

            For<IAgent>().Use<BoshAgent>();

            For<IMessageHandler>().AddInstances(x =>
                {
                    // agent/lib/agent/handler.rb
                    x.Type<Ping>().Named("ping");
                    x.Type<Noop>().Named("noop");
                    x.Type<Start>().Named("start");
                    x.Type<Stop>().Named("stop");
                    x.Type<PrepareNetworkChange>().Named("prepare_network_change");

                    x.Type<Apply>().Named("apply");
                    x.Type<CompilePackage>().Named("compile_package");
                    x.Type<Drain>().Named("drain");
                    x.Type<GetTask>().Named("get_task");
                    x.Type<State>().Named("state");
                    x.Type<Shutdown>().Named("shutdown");
                }
            );
        }
    }
}