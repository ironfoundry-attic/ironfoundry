namespace IronFoundry.Vcap
{
    using System;

    public class Constants
    {
        public static readonly Uri DEFAULT_TARGET       = new Uri("http://api.cloudfoundry.com");
        public static readonly Uri DEFAULT_LOCAL_TARGET = new Uri("http://api.vcap.me");

        // General Paths
        public const string INFO_PATH            = "/info";
        public const string GLOBAL_SERVICES_PATH = "/info/services";
        public const string RESOURCES_PATH       = "/resources";

        // User specific paths
        public const string APPS_PATH     = "/apps";
        public const string SERVICES_PATH = "/services";
        public const string USERS_PATH    = "/users";
    }
}
