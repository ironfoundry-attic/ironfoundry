namespace IronFoundry.Dea.IoC
{
    using System;
    using System.ServiceProcess;
    using IronFoundry.Dea.WinService;
    using IronFoundry.Misc.Logging;
    using StructureMap;

    public static class Bootstrapper
    {
        private static bool bootstrapped = false;

        static Bootstrapper()
        {
            ObjectFactory.Initialize(init => init.AddRegistry<ServiceRegistry>());
        }

        public static ServiceBase ServiceBase
        {
            get
            {
                if (false == bootstrapped)
                {
                    throw new InvalidOperationException("Must call Bootstrapper.Bootstrap() first.");
                }
                return ObjectFactory.GetInstance<IMultipleServiceManager>() as ServiceBase;
            }
        }

        public static IMultipleServiceManager ServiceManager
        {
            get
            {
                if (false == bootstrapped)
                {
                    throw new InvalidOperationException("Must call Bootstrapper.Bootstrap() first.");
                }
                return ObjectFactory.GetInstance<IMultipleServiceManager>();
            }
        }

        public static ILog Logger
        {
            get
            {
                if (false == bootstrapped)
                {
                    throw new InvalidOperationException("Must call Bootstrapper.Bootstrap() first.");
                }
                return ObjectFactory.GetInstance<ILog>();
            }
        }

        public static void Bootstrap()
        {
            bootstrapped = true;
        }
    }
}