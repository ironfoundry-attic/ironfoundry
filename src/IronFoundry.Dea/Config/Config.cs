namespace IronFoundry.Dea.Config
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Net.Sockets;

    public class Config : IConfig
    {
        private readonly DeaSection deaSection;
        private readonly IPAddress localIPAddress;

        private readonly Uri filesServiceUri;
        private readonly Uri monitoringServiceUri;

        private readonly ServiceCredentials filesCredentials;
        private readonly ServiceCredentials monitoringCredentials;

        private readonly ushort monitoringServicePort;

        public Config()
        {
            this.deaSection = (DeaSection)ConfigurationManager.GetSection(DeaSection.SectionName);
            this.localIPAddress = GetLocalIPAddress();

            this.filesServiceUri = new Uri(String.Format("http://localhost:{0}", FilesServicePort));

            this.monitoringServicePort = Utility.RandomFreePort();
            this.monitoringServiceUri = new Uri(String.Format("http://localhost:{0}", MonitoringServicePort));

            this.filesCredentials = new ServiceCredentials();
            this.monitoringCredentials = new ServiceCredentials();
        }

        public ushort MaxMemoryMB
        {
            get { return deaSection.MaxMemoryMB; }
        }

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

        public ushort MonitoringServicePort
        {
            get { return monitoringServicePort; }
        }

        public ServiceCredentials FilesCredentials
        {
            get { return filesCredentials; }
        }

        public ServiceCredentials MonitoringCredentials
        {
            get { return monitoringCredentials; }
        }

        public IPAddress LocalIPAddress
        {
            get { return localIPAddress; }
        }

        public Uri FilesServiceUri
        {
            get { return filesServiceUri; }
        }

        public Uri MonitoringServiceUri
        {
            get { return monitoringServiceUri; }
        }

        private IPAddress GetLocalIPAddress()
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Connect(deaSection.LocalRoute, 1);
                IPEndPoint ep = (IPEndPoint)udpClient.Client.LocalEndPoint;
                return ep.Address;
            }
        }
    }
}