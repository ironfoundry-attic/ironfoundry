namespace IronFoundry.Dea.Providers
{
    using System;
    using System.Linq;
    using System.Net;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Services;
    using Microsoft.Web.Administration;

    public class WebServerAdministrationProvider : IWebServerAdministrationProvider
    {
        private readonly ILog log;
        private readonly IPAddress localIPAddress;
        private readonly IFirewallService firewallService;
        private const ushort STARTING_PORT = 9000;

        public WebServerAdministrationProvider(ILog log, IConfig config, IFirewallService firewallService)
        {
            this.log = log;
            this.localIPAddress = config.LocalIPAddress;
            this.firewallService = firewallService;
        }

        public WebServerAdministrationBinding InstallWebApp(string localDirectory, string applicationInstanceName)
        {
            WebServerAdministrationBinding rv = null;

            try
            {
                using (var manager = new ServerManager())
                {
                    ApplicationPool cloudFoundryPool = GetApplicationPool(manager, applicationInstanceName);
                    if (null == cloudFoundryPool)
                    {
                        cloudFoundryPool = manager.ApplicationPools.Add(applicationInstanceName);
                    }

                    ushort applicationPort = FindNextAvailablePort();

                    firewallService.Open(applicationPort, applicationInstanceName);

                    /*
                     * NB: for now, listen on all local IPs, a specific port, and any domain.
                     * TODO: should we limit by host header here?
                     * TODO: use local IP here?
                     */
                    manager.Sites.Add(applicationInstanceName, "http", "*:" + applicationPort.ToString() + ":", localDirectory);

                    manager.Sites[applicationInstanceName].Applications[0].ApplicationPoolName = applicationInstanceName;

                    cloudFoundryPool.ManagedRuntimeVersion = "v4.0";

                    manager.CommitChanges();

                    rv = new WebServerAdministrationBinding() { Host = localIPAddress.ToString(), Port = applicationPort };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return rv;
        }

        public void UninstallWebApp(string applicationInstanceName)
        {
            try
            {
                using (var manager = new ServerManager())
                {
                    Site site = GetSite(manager, applicationInstanceName);
                    if (null != site)
                    {
                        manager.Sites.Remove(site);
                    }
                    ApplicationPool applicationPool = GetApplicationPool(manager, applicationInstanceName);
                    if (null != applicationPool)
                    {
                        manager.ApplicationPools.Remove(applicationPool);
                    }
                    manager.CommitChanges();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            try
            {
                firewallService.Close(applicationInstanceName);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public bool DoesApplicationExist(string applicationInstanceName)
        {
            bool rv = false;

            try
            {
                using (var manager = new ServerManager())
                {
                    var site = GetSite(manager, applicationInstanceName);
                    rv = site != null;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return rv;
        }

        public void Start(string applicationInstanceName)
        {
            try
            {
                using (var manager = new ServerManager())
                {
                    ApplicationPool applicationPool = GetApplicationPool(manager, applicationInstanceName);
                    applicationPool.Start();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void Stop(string applicationInstanceName)
        {
            try
            {
                using (var manager = new ServerManager())
                {
                    var applicationPool = GetApplicationPool(manager, applicationInstanceName);
                    applicationPool.Stop();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void Restart(string applicationInstanceName)
        {
            try
            {
                using (var manager = new ServerManager())
                {
                    var applicationPool = GetApplicationPool(manager, applicationInstanceName);
                    applicationPool.Recycle();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public ApplicationInstanceStatus GetStatus(string applicationInstanceName)
        {
            ApplicationInstanceStatus rv = ApplicationInstanceStatus.Unknown;

            try
            {
                using (var manager = new ServerManager())
                {
                    ApplicationPool applicationPool = GetApplicationPool(manager, applicationInstanceName);
                    Site applicationSite = GetSite(manager, applicationInstanceName);
                    if (applicationSite.State == ObjectState.Stopped || applicationPool.State == ObjectState.Stopped)
                    {
                        rv = ApplicationInstanceStatus.Stopped;
                    }
                    else if (applicationSite.State == ObjectState.Stopping || applicationPool.State == ObjectState.Stopping)
                    {
                        rv = ApplicationInstanceStatus.Stopping;
                    }
                    else if (applicationSite.State == ObjectState.Starting || applicationPool.State == ObjectState.Starting)
                    {
                        rv = ApplicationInstanceStatus.Starting;
                    }
                    else if (applicationSite.State == ObjectState.Started || applicationPool.State == ObjectState.Started)
                    {
                        rv = ApplicationInstanceStatus.Started;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return rv;
        }

        private static ApplicationPool GetApplicationPool(ServerManager argManager, string argName)
        {
            return argManager.ApplicationPools.FirstOrDefault(a => a.Name == argName);
        }

        private static Site GetSite(ServerManager argManager, string argName)
        {
            return argManager.Sites.FirstOrDefault(s => s.Name == argName);
        }

        private static ushort FindNextAvailablePort()
        {
            return Utility.FindNextAvailablePortAfter(STARTING_PORT);
        }
    }
}