using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronFoundry.Warden.IISHost
{
    public static class Constants
    {
        public static class ConfigXPath
        {
            public static string Configuration = "/configuration";

            public static string ApplicationHost = Configuration + "/system.applicationHost";
            public static string AppPools = ApplicationHost + "/applicationPools";
            public static string Sites = ApplicationHost + "/sites";
            public static string SiteDefaults = Sites + "/siteDefaults";

            public static string WebServer = Configuration + "/system.webServer";

            public static class LocationSpecific
            {
                public static string Location = Configuration + "/location";
                public static string SystemDotWeb = Location + "/system.web";
            }
        }

        public static class FrameworkPaths
        {
            public static string TwoDotZero = @"%windir%\Microsoft.NET\Framework\v2.0.50727";
            public static string FourDotZero = @"%windir%\Microsoft.NET\Framework\v4.0.30319";

            public static string TwoDotZeroWebConfig = TwoDotZero + @"\Config\web.config";
            public static string FourDotZeroWebConfig = FourDotZero + @"\Config\web.config";
        }

        public static class RuntimeVersion
        {
            public static string VersionFourDotZero = "v4.0";
            public static string VersionTwoDotZero = "v2.0";
        }

        public static class PipelineMode
        {
            public static string Integrated = "Integrated";
            public static string Classic = "Classic";
        }
    }
}
