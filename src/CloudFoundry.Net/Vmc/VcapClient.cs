namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Types;

    public class VcapClient : IVcapClient
    {
        private readonly VcapCredentialManager credentialManager;

        public VcapClient()
        {
            credentialManager = new VcapCredentialManager();
        }

        public VcapClient(string argUri)
        {
            credentialManager = new VcapCredentialManager();
            credentialManager.SetTarget(argUri);
        }

        public string CurrentUri
        {
            get { return credentialManager.CurrentTarget.AbsoluteUriTrimmed(); }
        }

        public VcapClientResult Info()
        {
            var helper = new MiscHelper();
            return helper.Info();
        }

        public VcapClientResult Target(string argUri)
        {
            var helper = new MiscHelper();

            if (argUri.IsNullOrWhiteSpace())
            {
                return helper.Target(null);
            }
            else
            {
                return helper.Target(new Uri(argUri));
            }
        }

        public VcapClientResult Login(Cloud argCloud)
        {
            var helper = new UserHelper();
            return helper.Login(argCloud);
        }

        public VcapClientResult Login(string argEmail, string argPassword)
        {
            var helper = new UserHelper();
            return helper.Login(argEmail, argPassword);
        }

        public VcapClientResult Push(Cloud argCloud, string argName, string argDeployFQDN, DirectoryInfo argPath, uint argMemory)
        {
            return doPush(new Uri(argCloud.Url), argName, argDeployFQDN, argPath, argMemory);
        }

        public VcapClientResult Push(string argName, string argDeployFQDN, DirectoryInfo argPath, uint argMemory)
        {
            return doPush(credentialManager.CurrentTarget, argName, argDeployFQDN, argPath, argMemory);
        }

        public void Start(Cloud argCloud, Application argApplication)
        {
            var apps = new AppsHelper();
            apps.Start(argCloud, argApplication);
        }

        public void Stop(Cloud argCloud, Application argApplication)
        {
            var apps = new AppsHelper();
            apps.Stop(argCloud, argApplication);
        }

        public Application GetAppInfo(Cloud argCloud, string argName)
        {
            var app = new AppsHelper();
            return app.GetAppInfo(argCloud, argName);
        }

        public VcapResponse UpdateApplicationSettings(Application application, Cloud cloud)
        {
            var app = new AppsHelper();
            return app.UpdateApplicationSettings(cloud, application);
        }


        public void RestartApp(Application application, Cloud cloud)
        {
            var app = new AppsHelper();
            app.RestartApp(application, cloud);
        }

        public string GetLogs(Application application, int instanceNumber, Cloud cloud)
        {
            var info = new InfoHelper();
            return info.GetLogs(application, instanceNumber, cloud);
        }

        public IEnumerable<StatInfo> GetStats(Application argApplication, Cloud argCloud)
        {
            var info = new InfoHelper();
            return info.GetStats(argApplication, argCloud);
        }

        public IEnumerable<ExternalInstance> GetInstances(Application argApplication, Cloud argCloud)
        {
            var info = new InfoHelper();
            return info.GetInstances(argApplication, argCloud);
        }

        public IEnumerable<Crash> GetAppCrash(Application argApplication, Cloud argCloud)
        {
            var apps = new AppsHelper();
            return apps.GetAppCrash(argApplication, argCloud);
        }

        public IEnumerable<Application> ListApps(Cloud argCloud)
        {
            var apps = new AppsHelper();
            return apps.ListApps(argCloud);
        }

        public IEnumerable<SystemServices> GetAvailableServices(Cloud cloud)
        {
            var services = new ServicesHelper();
            return services.GetAvailableServices(cloud);
        }

        public IEnumerable<ProvisionedService> GetProvisionedServices(Cloud argCloud)
        {
            var services = new ServicesHelper();
            return services.GetProvisionedServices(argCloud);
        }

        private VcapClientResult checkLoginStatus()
        {
            return Info();
        }

        private VcapClientResult doPush(Uri argUri, string argName, string argDeployFQDN, DirectoryInfo argPath, uint argMemory)
        {
            VcapClientResult rv = checkLoginStatus();

            if (rv.Success)
            {
                var apps = new AppsHelper();
                string app =  apps.Push(argName, argPath, argDeployFQDN, "ASP.NET 4.0", "aspdotnet", argMemory, null);
                rv = new VcapClientResult(true, app);
            }
            else
            {
                // TODO
            }

            return rv;
        }
    }
}