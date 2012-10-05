namespace IronFoundry.Bosh.IoC
{
    using IronFoundry.Bosh.Agent;
    using IronFoundry.Bosh.Agent.Handlers;
    using IronFoundry.Bosh.Configuration;
    using IronFoundry.Misc.Agent;
    using IronFoundry.Nats.Client;
    using StructureMap.Configuration.DSL;

    public class BoshRegistry : Registry
    {
        public BoshRegistry()
        {
            For<IAgent>().Use<BoshAgent>();

            For<IBoshConfig>().Singleton().Use<BoshConfig>();

            For<INatsClient>().Singleton().Use<NatsClient>();

            For<IMessageHandler>().AddInstances(x =>
                {
                    // agent/lib/agent/handler.rb
                    x.Type<Ping>().Named("ping");
                    x.Type<Noop>().Named("noop");
                    x.Type<Start>().Named("start");
                    x.Type<Stop>().Named("stop");
                    x.Type<PrepareNetworkChange>().Named("prepare_network_change");

                    x.Type<Ssh>().Named("ssh");

                    x.Type<FetchLogs>().Named("fetch_logs");

                    x.Type<MigrateDisk>().Named("migrate_disk");
                    x.Type<ListDisk>().Named("list_disk");
                    x.Type<MountDisk>().Named("mount_disk");
                    x.Type<UnmountDisk>().Named("unmount_disk");

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