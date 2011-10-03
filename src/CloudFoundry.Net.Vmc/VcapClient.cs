namespace CloudFoundry.Net.Vmc
{
    using System.Collections.Generic;
    using RestSharp;
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
            get { return credentialManager.CurrentTarget.AbsoluteUri; }
        }

        public VcapClientResult Info()
        {
            string response = executeRequest("/info");
            return new VcapClientResult(true, EntityBase.FromJson<Info>(response));
        }

        public VcapClientResult Target(string argUri)
        {
            VcapClientResult rv;

            if (argUri.IsNullOrWhiteSpace())
            {
                // Just return current target
                rv = new VcapClientResult(false, CurrentUri);
            }
            else
            {
                // "target" does the same thing as "info", but not logged in
                // considered valid if name, build, version and support are all non-null
                // without argument, displays current target
                var info = EntityBase.FromJson<Info>(executeRequest("/info", false));
                bool success = false == info.Name.IsNullOrWhiteSpace() &&
                               false == info.Build.IsNullOrWhiteSpace() &&
                               false == info.Version.IsNullOrWhiteSpace() &&
                               false == info.Support.IsNullOrWhiteSpace();

                if (success)
                {
                    credentialManager.SetTarget(argUri);
                    credentialManager.StoreTarget();
                }

                rv = new VcapClientResult(success);
            }

            return rv;
        }

        public VcapClientResult Login(string email, string password)
        {
            var cfa = new VmcAdministration();
            string result = cfa.Login(email, password, CurrentUri);
            credentialManager.RegisterFor(CurrentUri, result);
            return new VcapClientResult();
        }

        public VcapClientResult Login(Cloud cloud)
        {
            VmcAdministration cfa = new VmcAdministration();
            return new VcapClientResult(cfa.Login(cloud));
        }
        
        public string Push(string appname, string fdqn, string fileURI, string framework, string memorysize)
        {
            VmcApps cfapps = new VmcApps();
            var app =  cfapps.PushApp(appname, credentialManager.CurrentTarget, credentialManager.CurrentToken, fileURI, fdqn, framework, null,memorysize, null);
            return app;
        }

        public void StopApp(Application application, Cloud cloud)
        {
            VmcApps apps = new VmcApps();
            apps.StopApp(application, cloud);
        }

        public void StartApp(Application application, Cloud cloud)
        {
            VmcApps apps = new VmcApps();
            apps.StartApp(application, cloud);
        }

        public Application GetAppInfo(string appname, Cloud cloud)
        {
            VmcApps app = new VmcApps();
            return app.GetAppInfo(appname, cloud);
        }

        public VcapResponse UpdateApplicationSettings(Application application, Cloud cloud)
        {
            VmcApps app = new VmcApps();
            return app.UpdateApplicationSettings(application, cloud);
        }


        public void RestartApp(Application application, Cloud cloud)
        {
            VmcApps app = new VmcApps();
            app.RestartApp(application, cloud);
        }


        public string GetLogs(Application application, int instanceNumber, Cloud cloud)
        {
            VmcInfo info = new VmcInfo();
            return info.GetLogs(application, instanceNumber, cloud);
        }

        public SortedDictionary<int,StatInfo> GetStats(Application application, Cloud cloud)
        {
            VmcInfo info = new VmcInfo();
            return info.GetStats(application, cloud);
        }

        public List<ExternalInstance> GetInstances(Application application, Cloud cloud)
        {
            VmcInfo info = new VmcInfo();
            return info.GetInstances(application, cloud);
        }

        public List<Crash> GetAppCrash(Application application, Cloud cloud)
        {
            VmcApps apps = new VmcApps();
            return apps.GetAppCrash(application, cloud);
        }

        public List<Application> ListApps(Cloud cloud)
        {
            VmcApps apps = new VmcApps();
            return apps.ListApps(cloud);
        }

        public List<SystemServices> GetAvailableServices(Cloud cloud)
        {
            VmcServices services = new VmcServices();
            return services.GetAvailableServices(cloud);
        }

        public List<AppService> GetProvisionedServices(Cloud cloud)
        {
            VmcServices services = new VmcServices();
            return services.GetProvisionedServices(cloud);
        }

        private string executeRequest(string argResource, bool argUseCredentials = true)
        {
            var client = new RestClient { BaseUrl = credentialManager.CurrentTarget.AbsoluteUri };
            var request = new RestRequest { Resource = argResource };
            if (argUseCredentials && credentialManager.HasToken)
            {
                request.AddHeader("Authorization", credentialManager.CurrentToken);
            }
            return client.Execute(request).Content;
        }
    }
}