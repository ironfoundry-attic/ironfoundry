namespace IronFoundry.Dea.Config
{
    using System.Configuration;

    public static class DeaConfig
    {
        private static readonly DeaSection deaSection = (DeaSection)ConfigurationManager.GetSection(DeaSection.SectionName);

        public static bool DisableDirCleanup
        {
            get { return deaSection.DisableDirCleanup; }
        }

        public static string DropletDir
        {
            get { return deaSection.DropletDir; }
        }

        public static string AppDir
        {
            get { return deaSection.AppDir; }
        }

        public static string NatsHost
        {
            get { return deaSection.NatsHost; }
        }

        public static ushort NatsPort
        {
            get { return deaSection.NatsPort; }
        }
    }
}