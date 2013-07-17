namespace IronFoundry.Warden.ProcessIsolationService
{
    using System;
    using System.IO;
    using System.ServiceProcess;
    using ProcessIsolation;

    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

#if DEBUG // only allow command line service run in debug build.
            if (Environment.UserInteractive)
            {
                var service = new SelfHostedProcessHostService();
                service.StartService(args);
                Console.WriteLine(@"Hit [Enter] key to stop service...");
                Console.ReadLine();
                service.StopService();
            }
            else
#endif
                ServiceBase.Run(new[] { new SelfHostedProcessHostService() });
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var genericMessage = String.Format("Unhandled exception in AppDomain '{0}'", AppDomain.CurrentDomain.FriendlyName);
            var ex = e.ExceptionObject as Exception;

            if (ex != null)
            {
                EventLogger.ProcessHost.Error(genericMessage, ex);
            }
            else if (e.ExceptionObject != null)
            {
                EventLogger.ProcessHost.Error(genericMessage, e.ExceptionObject);
            }
            else
            {
                EventLogger.ProcessHost.Error(genericMessage);
            }
        }
    }
}
