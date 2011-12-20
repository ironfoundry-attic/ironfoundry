namespace IronFoundry.Dea.Providers
{
    using System.Linq;
    using System.Net;
    using IronFoundry.Dea.Config;
    using Microsoft.Web.Administration;

    public class WebServerAdministrationProvider : IWebServerAdministrationProvider
    {
        private readonly IPAddress localIPAddress;
        private const ushort STARTING_PORT = 9000;

        public WebServerAdministrationProvider(IConfig config)
        {
            this.localIPAddress = config.LocalIPAddress;
        }

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
                ApplicationPool applicationPool = getApplicationPool(manager, applicationInstanceName);
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
                    ApplicationPool applicationPool = getApplicationPool(manager, applicationInstanceName);
                    Site applicationSite = getSite(manager, applicationInstanceName);
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


        private static ApplicationPool getApplicationPool(ServerManager argManager, string argName)
        {
            return argManager.ApplicationPools.FirstOrDefault(a => a.Name == argName);
        }

        private static Site getSite(ServerManager argManager, string argName)
        {
            return argManager.Sites.FirstOrDefault(s => s.Name == argName);
        }

        private static ushort findNextAvailablePort()
        {
            return Utility.FindNextAvailablePortAfter(STARTING_PORT);
        }
    }
}