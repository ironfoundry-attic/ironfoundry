namespace IronFoundry.Misc.WinService
{
    using System;
    using System.IO;
    using System.ServiceProcess;
    using IronFoundry.Misc.IoC;
    using IronFoundry.Misc.Logging;

    public static class ServiceMain
    {
        static readonly ILog log;

        static ServiceMain()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Bootstrapper.Bootstrap();
            log = Bootstrapper.Logger;
        }

        public static void Start(string[] args)
        {
            if (Environment.UserInteractive)
            {
                IMultipleServiceManager mgr = Bootstrapper.ServiceManager;
                mgr.StartServiceManager();
                log.Info("Hit enter to stop ...");
                Console.ReadLine();
                mgr.StopServiceManager();
            }
            else
            {
                ServiceBase.Run(new[] { Bootstrapper.GetServiceBase(args) });
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error("Unhandled exception in AppDomain '{0}'", AppDomain.CurrentDomain.FriendlyName);
            var ex = e.ExceptionObject as Exception;
            if (null != ex)
            {
                log.Error(ex);
            }
        }
    }
}