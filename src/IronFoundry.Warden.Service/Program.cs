namespace IronFoundry.Warden.Service
{
    using System;
    using System.IO;
    using NLog;
    using Topshelf;

    static class Program
    {
        const string ServiceName = "ironfoundry.warden";
        static readonly Logger log = LogManager.GetCurrentClassLogger();

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
