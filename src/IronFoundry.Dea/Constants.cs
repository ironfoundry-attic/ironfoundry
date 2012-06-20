namespace IronFoundry.Dea
{
    using System;
    using System.Linq;
    using System.Net;

    public static class Constants
    {
        public const string FilesServiceNamespace = @"http://ironfoundry.org/dea/filesservice";
        public const string MonitoringServiceNamespace = @"http://ironfoundry.org/dea/monitoringservice";

        public static readonly IPAddress LocalhostIP;

        public static int[] MemoryLimits = new int[6] { 64, 128, 256, 512, 1024, 2048 };

        private const string localhostStr = "127.0.0.1";

        private const string aspDotNetFramework = "aspdotnet";
        private static readonly string[] supportedRuntimes = new[] { "aspdotnet40", "clr20", "clr40" };
        private static readonly string[] supportedFrameworks = new[] { aspDotNetFramework, "standalone" };

        static Constants()
        {
            IPAddress.TryParse(localhostStr, out LocalhostIP);
        }

        public static bool IsSupportedRuntime(string runtime)
        {
            return (! String.IsNullOrWhiteSpace(runtime)) && supportedRuntimes.Contains(runtime);
        }

        public static bool IsSupportedFramework(string framework)
        {
            return (! String.IsNullOrWhiteSpace(framework)) && supportedFrameworks.Contains(framework);
        }

        public static string[] SupportedRuntimes
        {
            get { return supportedRuntimes; }
        }

        public static bool IsAspNet(string framework)
        {
            return aspDotNetFramework == framework;
        }
    }
}