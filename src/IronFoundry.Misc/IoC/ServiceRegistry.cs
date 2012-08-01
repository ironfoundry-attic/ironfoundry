namespace IronFoundry.Misc.IoC
{
    using System;
    using System.Reflection;
    using IronFoundry.Misc.Logging;
    using StructureMap.Configuration.DSL;
    using WinService;

    public class ServiceRegistry : Registry
    {
        private const string defaultLoggerName = "IronFoundry";

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

            For<ILog>().AlwaysUnique().Use(f =>
                {
                    ILog rv;

                    if (null == f.ParentType)
                    {
                        if (null != f.BuildStack && null != f.BuildStack.Current &&
                            null != f.BuildStack.Current.ConcreteType &&
                            false == f.BuildStack.Current.ConcreteType.Name.IsNullOrWhiteSpace())
                        {
                            rv = new NLogLogger(f.BuildStack.Current.ConcreteType.Name);
                        }
                        else
                        {
                            rv = new NLogLogger(defaultLoggerName);
                        }
                    }
                    else
                    {
                        rv= new NLogLogger(f.ParentType.FullName);
                    }

                    return rv;
                });
        }
    }
}