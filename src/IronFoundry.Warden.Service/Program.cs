namespace IronFoundry.Warden.Service
{
    using System;
    using System.IO;
    using NLog;
    using Topshelf;

    // http://stackoverflow.com/questions/227187/uac-need-for-console-application
    static class Program
    {
        const string ServiceName = "ironfoundry.warden";
        static readonly Logger log = LogManager.GetCurrentClassLogger();
        static readonly LogLevel[] allLogLevels = new[] {
            LogLevel.Fatal, LogLevel.Error, LogLevel.Warn,
            LogLevel.Info, LogLevel.Debug, LogLevel.Trace
        };

        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            log.Info("Current directory is: '{0}'", Directory.GetCurrentDirectory());

            HostFactory.Run(x =>
                {
                    x.Service<WinService>();
                    x.SetDescription("Iron Foundry Warden Service");
                    x.SetDisplayName(ServiceName);
                    x.StartAutomaticallyDelayed();
                    x.RunAsPrompt();
                    x.UseNLog();
                });
        }
    }
}
