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

            IAgent agent = new Agent();

#if DEBUG
            if (Environment.UserInteractive)
            {
                agent.Start();
                Console.WriteLine("Hit enter to stop ...");
                Console.ReadLine();
                agent.Stop();
            }
            else
#endif
                ServiceBase.Run((ServiceBase)agent);
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