namespace CloudFoundry.Net.Dea.Service
{
    using System;
    using System.ServiceProcess;
    using NLog;

    static class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#if DEBUG
            if (Environment.UserInteractive)
            {
                var svc = new DeaWindowsService();
                svc.StartService();
                Console.WriteLine("Hit enter to stop ...");
                Console.ReadLine();
                svc.StopService();
            }
            else
#endif
                ServiceBase.Run(new DeaWindowsService());
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Error("Unhandled exception in AppDomain '{0}'", AppDomain.CurrentDomain.FriendlyName);
            var ex = e.ExceptionObject as Exception;
            if (null != ex)
            {
                logger.Error("Exception:", ex);
            }
        }
    }
}