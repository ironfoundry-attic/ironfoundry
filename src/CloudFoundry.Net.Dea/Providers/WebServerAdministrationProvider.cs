namespace CloudFoundry.Net.Dea.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using CloudFoundry.Net.Dea.Providers.Interfaces;
    using Microsoft.Web.Administration;

    public class WebServerAdministrationProvider : IWebServerAdministrationProvider
    {        
        private Dictionary<string, bool> iptable = new Dictionary<string, bool>();

        public WebServerAdministrationProvider()
        {
            string availableIps = ConfigurationManager.AppSettings["AvailableIps"] as string;
            string[] ips = availableIps.Split(';');
            foreach (var ip in ips)
                iptable.Add(ip, false);
        }

        public WebServerAdministrationBinding InstallWebApp(string localDirectory, string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                ApplicationPool cloudFoundryPool = GetApplicationPool(manager, applicationInstanceName);

                if (cloudFoundryPool == null)
                    cloudFoundryPool = manager.ApplicationPools.Add(applicationInstanceName);

                ushort applicationPort = findNextAvailablePort(manager);

                var availableIp = iptable.Where((i) => i.Value == false).FirstOrDefault();
                iptable[availableIp.Key] = true;

                // manager.Sites.Add(applicationInstanceName, "http", availableIp.Key, localDirectory);
                manager.Sites.Add(applicationInstanceName, "http", "*:" + applicationPort.ToString() + ":", localDirectory);

                manager.Sites[applicationInstanceName].Applications[0].ApplicationPoolName = applicationInstanceName;

                cloudFoundryPool.ManagedRuntimeVersion = "v4.0";

                manager.CommitChanges();

                return new WebServerAdministrationBinding() { Host = availableIp.Key, Port = applicationPort };
            }
        }

        public void UninstallWebApp(string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                var site = GetSite(manager, applicationInstanceName);
                if (site != null)
                {
                    manager.Sites.Remove(site);
                    manager.CommitChanges();
                }
                var deletePool = GetApplicationPool(manager, applicationInstanceName);
                if (deletePool != null)
                {
                    manager.ApplicationPools.Remove(deletePool);
                    manager.CommitChanges();
                }
            }
        }

        public bool DoesApplicationExist(string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                var site = GetSite(manager, applicationInstanceName);
                return site != null;
            }
        }

        public void Start(string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                var applicationPool = GetApplicationPool(manager, applicationInstanceName);
                applicationPool.Start();
            }
        }

        public void Stop(string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                var applicationPool = GetApplicationPool(manager, applicationInstanceName);
                applicationPool.Stop();
            }
        }

        public void Restart(string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                var applicationPool = GetApplicationPool(manager, applicationInstanceName);
                applicationPool.Recycle();
            }
        }

        public ApplicationInstanceStatus GetStatus(string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                try
                {
                    var applicationPool = GetApplicationPool(manager, applicationInstanceName);
                    var applicationSite = GetSite(manager, applicationInstanceName);
                    if (applicationSite.State == ObjectState.Stopped ||
                        applicationPool.State == ObjectState.Stopped)
                        return ApplicationInstanceStatus.Stopped;
                    if (applicationSite.State == ObjectState.Stopping ||
                        applicationPool.State == ObjectState.Stopping)
                        return ApplicationInstanceStatus.Stopping;
                    if (applicationSite.State == ObjectState.Starting ||
                        applicationPool.State == ObjectState.Starting)
                        return ApplicationInstanceStatus.Starting;
                    if (applicationSite.State == ObjectState.Started ||
                        applicationPool.State == ObjectState.Started)
                        return ApplicationInstanceStatus.Started;
                }
                catch
                {
                    // TODO
                }
                return ApplicationInstanceStatus.Unknown;
            }
        }


        private static ApplicationPool GetApplicationPool(ServerManager argManager, string name)
        {
            ApplicationPool returnPool = null;
            foreach (var pool in argManager.ApplicationPools)
            {
                if (pool.Name.Equals(name))
                {
                    returnPool = pool;
                    break;
                }
            }
            return returnPool;
        }

        private static ushort findNextAvailablePort(ServerManager argManager)
        {
            var portsInUse = new List<ushort>();
            foreach (var site in argManager.Sites)
            {
                foreach (var binding in site.Bindings)
                {
                    ushort inUsePort;
                    var bindingInformation = binding.BindingInformation;
                    bindingInformation = bindingInformation.Replace(":", String.Empty).Replace("*", "");
                    if (UInt16.TryParse(bindingInformation, out inUsePort))
                        portsInUse.Add(inUsePort);
                }
            }

            ushort applicationPort = 9000;
            for (; applicationPort < 10000; applicationPort++)
                if (!portsInUse.Contains(applicationPort))
                    break;

            return applicationPort;
        }

        private static Site GetSite(ServerManager argManager, string name)
        {
            Site returnSite = null;
            foreach (var site in argManager.Sites)
            {
                if (site.Name.Equals(name))
                {
                    returnSite = site;
                    break;
                }
            }
            return returnSite;
        }
    }
}
