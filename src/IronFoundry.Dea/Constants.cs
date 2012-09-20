namespace IronFoundry.Dea
{
    using System;
    using System.Collections.Generic;
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
        private static readonly IDictionary<string, ushort> runtimeMap = new Dictionary<string, ushort>
        {
            { "aspdotnet40", 4 },
            { "clr20", 2 },
            { "clr40", 4 },
        };
        private static readonly string[] supportedFrameworks = new[] { aspDotNetFramework };

        static Constants()
        {
            IPAddress.TryParse(localhostStr, out LocalhostIP);
        }

        public static bool IsSupportedRuntime(string runtime)
        {
            return (! String.IsNullOrWhiteSpace(runtime)) && runtimeMap.ContainsKey(runtime);
        }

        public static bool IsSupportedFramework(string framework)
        {
            return (! String.IsNullOrWhiteSpace(framework)) && supportedFrameworks.Contains(framework);
        }

        public static string[] SupportedRuntimes
        {
            get { return runtimeMap.Keys.ToArray(); }
        }

        public static bool IsAspNet(string framework)
        {
            return aspDotNetFramework == framework;
        }

        public static ushort GetManagedRuntimeVersion(string runtime)
        {
            ushort rv = 0;
            runtimeMap.TryGetValue(runtime, out rv);
            return rv;
        }
    }
}