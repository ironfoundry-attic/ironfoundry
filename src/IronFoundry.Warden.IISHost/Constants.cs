namespace IronFoundry.Warden.IISHost
{
    public class Constants
    {
        public class ConfigXPath
        {
            public const string Configuration = "/configuration";

            public const string ApplicationHost = Configuration + "/system.applicationHost";
            public const string AppPools = ApplicationHost + "/applicationPools";
            public const string Sites = ApplicationHost + "/sites";
            public const string SiteDefaults = Sites + "/siteDefaults";

            public const string WebServer = Configuration + "/system.webServer";
        }

        public class FrameworkPaths
        {
            // TODO: from registry
            public const string TwoDotZero = @"%windir%\Microsoft.NET\Framework\v2.0.50727";
            public const string FourDotZero = @"%windir%\Microsoft.NET\Framework\v4.0.30319";

            public const string TwoDotZeroWebConfig = TwoDotZero + @"\Config\web.config";
            public const string FourDotZeroWebConfig = FourDotZero + @"\Config\web.config";
        }

        public class RuntimeVersion
        {
            public const string VersionFourDotZero = "v4.0";
            public const string VersionTwoDotZero = "v2.0";
        }

        public class PipelineMode
        {
            public const string Integrated = "Integrated";
            public const string Classic = "Classic";
        }
    }
}
