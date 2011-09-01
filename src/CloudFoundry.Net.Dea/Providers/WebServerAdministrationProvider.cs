using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Dea.Providers.Interfaces;
using Microsoft.Web.Administration;
using System.Configuration;

namespace CloudFoundry.Net.Dea.Providers
{
    public class WebServerAdministrationProvider : IWebServerAdministrationProvider
    {        
        private ServerManager manager;
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
            manager = new ServerManager();
            ApplicationPool cloudFoundryPool = GetApplicationPool(applicationInstanceName);
            if (cloudFoundryPool == null)
                cloudFoundryPool = manager.ApplicationPools.Add(applicationInstanceName);
            var applicationPort = FindNextAvailablePort();

            var availableIp = iptable.Where((i) => i.Value == false).FirstOrDefault();
            iptable[availableIp.Key] = true;            
            manager.Sites.Add(applicationInstanceName, "http",availableIp.Key,localDirectory);            
            manager.Sites[applicationInstanceName].Applications[0].ApplicationPoolName = applicationInstanceName;
            cloudFoundryPool.ManagedRuntimeVersion = "v4.0";
            manager.CommitChanges();
            var binding = new WebServerAdministrationBinding() { Host = availableIp.Key, Port = 80 };
            return binding;
        }

        public void UninstallWebApp(string applicationInstanceName)
        {
            manager = new ServerManager();
            var site = GetSite(applicationInstanceName);
            if (site != null)
            {
                manager.Sites.Remove(site);
                manager.CommitChanges();
            }
            var deletePool = GetApplicationPool(applicationInstanceName);
            if (deletePool != null)
            {
                manager.ApplicationPools.Remove(deletePool);
                manager.CommitChanges();
            }
        }

        public bool DoesApplicationExist(string applicationInstanceName)
        {
            manager = new ServerManager();
            var site = GetSite(applicationInstanceName);
            return site != null;
        }

        public void Start(string applicationInstanceName)
        {
            manager = new ServerManager();
            var applicationPool = GetApplicationPool(applicationInstanceName);
            applicationPool.Start();
        }

        public void Stop(string applicationInstanceName)
        {
            manager = new ServerManager();
            var applicationPool = GetApplicationPool(applicationInstanceName);
            applicationPool.Stop();
        }

        public void Restart(string applicationInstanceName)
        {
            manager = new ServerManager();
            var applicationPool = GetApplicationPool(applicationInstanceName);
            applicationPool.Recycle();
        }

        public ApplicationInstanceStatus GetStatus(string applicationInstanceName)
        {
            try
            {
                var applicationPool = GetApplicationPool(applicationInstanceName);
                var applicationSite = GetSite(applicationInstanceName);
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
            catch (Exception) { }
            return ApplicationInstanceStatus.Unknown;
        }


        private ApplicationPool GetApplicationPool(string name)
        {
            ApplicationPool returnPool = null;
            foreach (var pool in manager.ApplicationPools)
            {
                if (pool.Name.Equals(name))
                {
                    returnPool = pool;
                    break;
                }
            }
            return returnPool;
        }

        private int FindNextAvailablePort()
        {
            var portsInUse = new List<int>();
            foreach (var site in manager.Sites)
                foreach (var binding in site.Bindings)
                {
                    int inUsePort;
                    var bindingInformation = binding.BindingInformation;
                    bindingInformation = bindingInformation.Replace(":", string.Empty).Replace("*", "");
                    if (Int32.TryParse(bindingInformation, out inUsePort))
                        portsInUse.Add(inUsePort);
                }

            var applicationPort = 9000;
            for (; applicationPort < 10000; applicationPort++)
                if (!portsInUse.Contains(applicationPort))
                    break;

            return applicationPort;
        }

        private Site GetSite(string name)
        {
            Site returnSite = null;
            foreach (var site in manager.Sites)
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
