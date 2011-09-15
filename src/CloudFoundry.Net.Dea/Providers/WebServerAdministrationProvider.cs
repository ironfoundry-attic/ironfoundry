namespace CloudFoundry.Net.Dea.Providers
{
    using System.Net;
    using Microsoft.Web.Administration;

    public class WebServerAdministrationProvider : IWebServerAdministrationProvider
    {
        /*
         * TODO 
         * DEA connects to root DNS server via UDP and picks the last address.
         * Probably should figure out a way to specify the IP.
         */
        private readonly IPAddress localIPAddress = Utility.LocalIPAddress;

        public WebServerAdministrationBinding InstallWebApp(string localDirectory, string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                ApplicationPool cloudFoundryPool = getApplicationPool(manager, applicationInstanceName);

                if (cloudFoundryPool == null)
                    cloudFoundryPool = manager.ApplicationPools.Add(applicationInstanceName);

                ushort applicationPort = findNextAvailablePort();

                /*
                 * NB: for now, listen on all local IPs, a specific port, and any domain.
                 * TODO: should we limit by host header here?
                 * TODO: use local IP here?
                 */
                manager.Sites.Add(applicationInstanceName, "http", "*:" + applicationPort.ToString() + ":", localDirectory);

                manager.Sites[applicationInstanceName].Applications[0].ApplicationPoolName = applicationInstanceName;

                cloudFoundryPool.ManagedRuntimeVersion = "v4.0";

                manager.CommitChanges();

                return new WebServerAdministrationBinding() { Host = localIPAddress.ToString(), Port = applicationPort };
            }
        }

        public void UninstallWebApp(string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                var site = getSite(manager, applicationInstanceName);
                if (site != null)
                {
                    manager.Sites.Remove(site);
                    manager.CommitChanges();
                }
                var deletePool = getApplicationPool(manager, applicationInstanceName);
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
                var site = getSite(manager, applicationInstanceName);
                return site != null;
            }
        }

        public void Start(string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                var applicationPool = getApplicationPool(manager, applicationInstanceName);
                applicationPool.Start();
            }
        }

        public void Stop(string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                var applicationPool = getApplicationPool(manager, applicationInstanceName);
                applicationPool.Stop();
            }
        }

        public void Restart(string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                var applicationPool = getApplicationPool(manager, applicationInstanceName);
                applicationPool.Recycle();
            }
        }

        public ApplicationInstanceStatus GetStatus(string applicationInstanceName)
        {
            using (var manager = new ServerManager())
            {
                try
                {
                    var applicationPool = getApplicationPool(manager, applicationInstanceName);
                    var applicationSite = getSite(manager, applicationInstanceName);
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


        private static ApplicationPool getApplicationPool(ServerManager argManager, string name)
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

        private static ushort findNextAvailablePort()
        {
            return Utility.FindNextAvailablePortAfter(9000);
        }

        private static Site getSite(ServerManager argManager, string name)
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