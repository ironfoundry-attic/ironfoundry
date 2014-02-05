using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using IronFoundry.Misc;
using IronFoundry.Nats.Configuration;
using Microsoft.Win32;

namespace IronFoundry.Dea.Configuration
{
    public class DeaConfig : IDeaConfig
    {
        private readonly string appCmdPath;
        private readonly DeaSection deaSection;

        private readonly ServiceCredential filesCredentials;
        private readonly Uri filesServiceUri;
        private readonly bool hasAppCmd;
        private readonly IPAddress localIPAddress;
        private readonly ServiceCredential monitoringCredentials;

        private readonly string monitoringServiceHostStr;
        private readonly ushort monitoringServicePort;
        private readonly Uri monitoringServiceUri;

        public DeaConfig(INatsConfig natsConfig)
        {
            deaSection = (DeaSection) ConfigurationManager.GetSection(DeaSection.SectionName);
            localIPAddress = Utility.GetLocalIPAddress(deaSection.LocalRoute, natsConfig.Host);

            filesServiceUri = new Uri(String.Format("http://localhost:{0}", FilesServicePort));

            monitoringServicePort = Utility.RandomFreePort();
            monitoringServiceUri = new Uri(String.Format("http://localhost:{0}", MonitoringServicePort));
            monitoringServiceHostStr = String.Format(CultureInfo.InvariantCulture,
                "{0}:{1}", localIPAddress, monitoringServicePort);

            filesCredentials = new ServiceCredential();
            monitoringCredentials = new ServiceCredential();

            try
            {
                string iisInstallPath = string.Empty;
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default))
                {
                    using (RegistryKey subKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\InetStp"))
                    {
                        if (subKey != null) iisInstallPath = subKey.GetValue("InstallPath").ToString();
                    }
                }
                appCmdPath = Path.Combine(iisInstallPath, "appcmd.exe");
                if (File.Exists(appCmdPath))
                {
                    hasAppCmd = true;
                }
                else
                {
                    appCmdPath = null;
                    hasAppCmd = false;
                }
            }
            catch
            {
                appCmdPath = null;
                hasAppCmd = false;
            }
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

        public ushort FilesServicePort
        {
            get { return deaSection.FilesServicePort; }
        }

        public ushort MonitoringServicePort
        {
            get { return monitoringServicePort; }
        }

        public ServiceCredential FilesCredentials
        {
            get { return filesCredentials; }
        }

        public ServiceCredential MonitoringCredentials
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

        public string MonitoringServiceHostStr
        {
            get { return monitoringServiceHostStr; }
        }

        public string AppCmdPath
        {
            get { return appCmdPath; }
        }

        public bool HasAppCmd
        {
            get { return hasAppCmd; }
        }
    }
}