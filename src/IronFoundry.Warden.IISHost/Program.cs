namespace IronFoundry.Warden.IISHost
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using NLog;
    using System.Text;
    using CommandLine;
    using CommandLine.Text;

    internal static class Program
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var exitLatch = new ManualResetEvent(false);
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                exitLatch.Set();
            };

            try
            {
                var options = new Options();
                if (Parser.Default.ParseArguments(args, options))
                {
                    log.Info("Port:", options.Port);
                    log.Info("Webroot:", options.WebRoot);
                    log.Info("Runtime:", options.RuntimeVersion);
                }
                else
                {
                    log.Info(options.Usage());
                    Environment.Exit(1);
                }

                ConfigSettings settings;
                var configGenerator = new ConfigGenerator(options.WebRoot);
                switch (options.RuntimeVersion)
                {
                    case "2":
                    case "2.0":
                        settings = configGenerator.Create(
                            options.Port,
                            Constants.FrameworkPaths.TwoDotZeroWebConfig,
                            Constants.RuntimeVersion.VersionTwoDotZero,
                            Constants.PipelineMode.Integrated);
                        break;
                    default:
                        settings = configGenerator.Create(
                            options.Port,
                            Constants.FrameworkPaths.FourDotZeroWebConfig,
                            Constants.RuntimeVersion.VersionFourDotZero,
                            Constants.PipelineMode.Integrated);
                        break;
                }

                log.Info("starting web server instance...");
                using (var webServer = new WebServer(settings))
                {
                    webServer.Start();
                    Console.WriteLine("Server Started.... press CTRL + C to stop");

                    StartInBrowser(options);

                    exitLatch.WaitOne();
                    Console.WriteLine("Server shutting down, please wait...");
                    webServer.Stop();
                }
            }
            catch (Exception ex)
            {
                log.ErrorException("Error on startup.", ex);
                if (Environment.UserInteractive)
                {
                    Console.ReadLine();
                }
                Environment.Exit(2);
            }
        }

        private static void StartInBrowser(Options options)
        {
            try
            {
                if (Environment.UserInteractive && options.StartInBrowser)
                {
                    Process.Start(String.Format("http://localhost:{0}", options.Port));
                }
            }
            catch (Exception ex)
            {
                log.DebugException("Unable to start in browser", ex);
            }
        }
    }

    internal class Options
    {
        [Option('p', "port", Required = true, HelpText = "The port for the IIS website.")]
        public uint Port { get; set; }

        [Option('r', "webroot", Required = true, HelpText = "The local webroot path for website.")]
        public string WebRoot { get; set; }

        [Option('v', "runtimeVersion", Required = false, DefaultValue = "4.0", HelpText = "AppPool runtime version: 2.0 or 4.0")]
        public string RuntimeVersion { get; set; }

        [Option('b', "startInBrowser", Required = false, DefaultValue = false, HelpText = "Specify true to start a browser pointing to the site.")]
        public bool StartInBrowser { get; set; }

        [HelpOption]
        public string Usage()
        {
            return HelpText.AutoBuild(this, c => HelpText.DefaultParsingErrorsHandler(this, c));
        }
    }
}
