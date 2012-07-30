namespace WinService
{
    using System;
    using System.IO;
    using System.ServiceProcess;
    using IronFoundry.Dea.IoC;
    using IronFoundry.Dea.WinService;
    using IronFoundry.Misc.Logging;

    static class Program
    {
        static readonly ILog log;

        static Program()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Bootstrapper.Bootstrap();
            log = Bootstrapper.Logger;
        }

        static void Main(string[] args)
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
                ServiceBase.Run(new[] { Bootstrapper.ServiceBase });
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