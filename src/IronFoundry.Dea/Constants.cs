namespace IronFoundry.Dea
{
    using System.Net;

    public static class Constants
    {
        public const string FilesServiceNamespace = @"http://ironfoundry.org/dea/filesservice";
        public const string MonitoringServiceNamespace = @"http://ironfoundry.org/dea/monitoringservice";

        public const string SupportedFramework = "aspdotnet";

        public const string JsonDateFormat = "yyyy-MM-dd HH:mm:ss zz00";

        public static readonly IPAddress LocalhostIP;

        public static int[] MemoryLimits = new int[6] { 64, 128, 256, 512, 1024, 2048 };

        private const string localhostStr = "127.0.0.1";

        static Constants()
        {
            IPAddress.TryParse(localhostStr, out LocalhostIP);
        }
    }
}