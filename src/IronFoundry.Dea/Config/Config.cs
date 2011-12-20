namespace IronFoundry.Dea.Config
{
    using System.Configuration;

    public class Config : IConfig
    {
        private readonly DeaSection deaSection = (DeaSection)ConfigurationManager.GetSection(DeaSection.SectionName);

        public bool DisableDirCleanup
        {
            get { return deaSection.DisableDirCleanup; }
        }

        public string DropletDir
        {
            get { return deaSection.DropletDir; }
        }

        public string AppDir
        {
            get { return deaSection.AppDir; }
        }

        public string NatsHost
        {
            get { return deaSection.NatsHost; }
        }

        public ushort NatsPort
        {
            get { return deaSection.NatsPort; }
        }

        public ushort FilesServicePort
        {
            get { return deaSection.FilesServicePort; }
        }
    }
}