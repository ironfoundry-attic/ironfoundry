namespace CloudFoundry.Net.Vmc
{
    using System.Collections.Generic;
    using RestSharp;
    using Types;

    public class VcapClient : IVcapClient
    {
        private readonly AccessTokenManager tokenManager = new AccessTokenManager();

        private readonly string currentUri;

        private AccessToken currentToken;

        public VcapClient()
        {
            currentToken = tokenManager.GetFirst();
            currentUri = currentToken.Uri.AbsoluteUri;
        }

        public VcapClient(string argUri)
        {
            currentUri = argUri;
            currentToken = tokenManager.GetFor(argUri);
        }

        public VcapClientResult Login(string email, string password)
        {
            var cfa = new VmcAdministration();
            string result = cfa.Login(email, password, currentUri);
            currentToken = tokenManager.CreateFor(currentUri, result);
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
            var app =  cfapps.PushApp(appname, currentToken.Uri, currentToken.Token, fileURI, fdqn, framework, null,memorysize, null);
            return app;
        }

        public VcapClientResult Info()
        {
            return new VcapClientResult(true, executeRequest("/info"));
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

        private string executeRequest(string argResource)
        {
            var client = new RestClient { BaseUrl = currentToken.Uri.AbsoluteUri };
            var request = new RestRequest { Resource = argResource };
            if (null != currentToken && false == currentToken.Token.IsNullOrWhiteSpace())
            {
                request.AddHeader("Authorization", currentToken.Token);
            }
            return client.Execute(request).Content;
        }
    }
}