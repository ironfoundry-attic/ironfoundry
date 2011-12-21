namespace IronFoundry.Dea.Config
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Net.Sockets;

    public class Config : IConfig
    {
        private readonly DeaSection deaSection;
        private readonly FilesServiceCredentials filesCredentials;
        private readonly IPAddress localIPAddress;
        private readonly Uri filesServiceUri;
        private readonly Uri wcfFilesServiceUri;

        public Config()
        {
            this.deaSection = (DeaSection)ConfigurationManager.GetSection(DeaSection.SectionName);
            this.filesCredentials = new FilesServiceCredentials();
            this.localIPAddress = GetLocalIPAddress();
            this.filesServiceUri = new Uri(String.Format("http://{0}:{1}", localIPAddress, FilesServicePort));
            this.wcfFilesServiceUri = new Uri(String.Format("http://localhost:{0}", FilesServicePort));
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

        public FilesServiceCredentials FilesCredentials
        {
            get { return filesCredentials; }
        }

        public IPAddress LocalIPAddress
        {
            get { return localIPAddress; }
        }

        public Uri FilesServiceUri
        {
            get { return filesServiceUri; }
        }

        public Uri WCFFilesServiceUri
        {
            get { return wcfFilesServiceUri; }
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