namespace CloudFoundry.Net
{
    using System.Net;

    public static class Constants
    {
        public const string SupportedFramework = "aspdotnet";

        public const string JsonDateFormat = "yyyy-MM-dd HH:mm:ss zz00";

        public static readonly IPAddress LocalhostIP;

        private const string localhostStr = "127.0.0.1";

        static Constants()
        {
            IPAddress.TryParse(localhostStr, out LocalhostIP);
        }

        public static class AppSettings
        {
            public const string StagingDirectory      = "StagingDirectory";
            public const string ApplicationsDirectory = "ApplicationsDirectory";
            public const string DropletsDirectory     = "DropletsDirectory";
            public const string DisableDirCleanup     = "DisableDirCleanup";
            public const string NatsHost              = "NatsHost";
            public const string NatsPort              = "NatsPort";
        }
    }
}