namespace IronFoundry.Dea.Config
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;

    public class Config : IConfig
    {
        private readonly DeaSection deaSection = (DeaSection)ConfigurationManager.GetSection(DeaSection.SectionName);
        private readonly FilesServiceCredentials filesCredentials = new FilesServiceCredentials();
        private readonly IPAddress localIPAddress = GetLocalIPAddresses().Last();

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

        public FilesServiceCredentials FilesCredentials
        {
            get { return filesCredentials; }
        }

        public IPAddress LocalIPAddress
        {
            get { return localIPAddress; }
        }

        private static IEnumerable<IPAddress> GetLocalIPAddresses()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToListOrNull();
        }
    }
}