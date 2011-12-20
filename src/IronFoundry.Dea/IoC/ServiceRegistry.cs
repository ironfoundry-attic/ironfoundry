namespace IronFoundry.Dea.IoC
{
    using System;
    using System.Reflection;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Providers;
    using StructureMap.Configuration.DSL;
    using WinService;

    public class ServiceRegistry : Registry
    {
        private const string defaultLoggerName = "IronFoundry.Dea.Service";

        private readonly Predicate<Assembly> skipStructureMap =
            (assm) => false == assm.FullName.StartsWith("StructureMap", StringComparison.InvariantCultureIgnoreCase);

        public ServiceRegistry()
        {
            Scan(s =>
            {
                s.WithDefaultConventions();
                s.TheCallingAssembly();
                s.AssembliesFromApplicationBaseDirectory(skipStructureMap);
                s.LookForRegistries();
                s.AddAllTypesOf<IService>();
            });

            For<IConfig>().Singleton().Use<Config>();

            For<IMessagingProvider>().Use<NatsMessagingProvider>();

            For<ILog>().Use(f =>
                {
                    if (null == f.ParentType)
                    {
                        return new NLogLogger(defaultLoggerName);
                    }
                    else
                    {
                        return new NLogLogger(f.ParentType.FullName);
                    }
                });
        }
    }
}