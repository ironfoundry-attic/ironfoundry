namespace IronFoundry.Warden.Service
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using Topshelf;

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

            SetupNLog();

            HostFactory.Run(x =>
                {
                    x.Service<WinService>();
                    x.SetDescription("Iron Foundry Warden Service");
                    x.SetDisplayName(ServiceName);
                    x.StartAutomaticallyDelayed();
                    x.RunAsLocalService();
                    x.UseNLog();
                });
        }

        private static void SetupNLog()
        {
            try
            {
                if (!Environment.UserInteractive)
                {
                    LoggingConfiguration config = LogManager.Configuration;
                    var consoleRules = config.LoggingRules.Where(r => r.Targets.Any(t => t.GetType() == typeof(ConsoleTarget))).ToArrayOrNull();
                    if (consoleRules != null)
                    {
                        foreach (var rule in consoleRules)
                        {
                            foreach (var level in allLogLevels)
                            {
                                if (rule.IsLoggingEnabledForLevel(level))
                                {
                                    rule.DisableLoggingForLevel(level);
                                }
                            }
                        }
                    }
                    LogManager.ReconfigExistingLoggers();
                }
            }
            catch (Exception ex)
            {
                log.ErrorException(ex.Message, ex);
                Environment.Exit(1);
            }
            log.Debug("NLog setup is complete.");
        }
    }
}
