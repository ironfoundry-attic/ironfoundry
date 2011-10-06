namespace CloudFoundry.Net.Vmc
{
    using System.Collections.Generic;
    using System.Linq;
    using CloudFoundry.Net.Types;
    using Newtonsoft.Json;
    using RestSharp;

    internal class ServicesHelper : BaseVmcHelper
    {
        public ServicesHelper(VcapCredentialManager credMgr) : base(credMgr) { }

        public IEnumerable<SystemService> GetSystemServices()
        {
            var r = new VcapRequest(credMgr,  Constants.GLOBAL_SERVICES_PATH);
            RestResponse response = r.Execute();

            var datastores = new List<SystemService>();
            var list = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, SystemService>>>>(response.Content);
            foreach (var val in list.Values)
            {
                foreach (var val1 in val.Values)
                {
                    foreach (var val2 in val1.Values)
                    {
                        datastores.Add(val2);
                    }
                }
            }

            return datastores.ToArrayOrNull(); 
        }

        public IEnumerable<ProvisionedService> GetProvisionedServices()
        {            
            var r = new VcapRequest(credMgr,  Constants.SERVICES_PATH);
            return r.Execute<ProvisionedService[]>();
        }

        public VcapClientResult CreateService(string argServiceName, string argProvisionedServiceName)
        {
            VcapClientResult rv;

            IEnumerable<SystemService> services = GetSystemServices();
            if (services.IsNullOrEmpty())
            {
                rv = new VcapClientResult(false);
            }
            else
            {
                SystemService svc = services.FirstOrDefault(s => s.Vendor == argServiceName);
                if (null == svc)
                {
                    rv = new VcapClientResult(false);
                }
                else
                {
                    // from vmc client.rb
                    var data = new
                    {
                        name    = argProvisionedServiceName,
                        type    = svc.Type,
                        tier    = "free",
                        vendor  = svc.Vendor,
                        version = svc.Version,
                    };
                    var r = new VcapJsonRequest(credMgr, Method.POST, data, Constants.SERVICES_PATH);
                    RestResponse response = r.Execute();
                    rv = new VcapClientResult();
                }
            }

            return rv;
        }

        public VcapClientResult DeleteService(string argProvisionedServiceName)
        {
            var request = new VcapJsonRequest(credMgr, Method.DELETE, Constants.SERVICES_PATH, argProvisionedServiceName);
            request.Execute();
            return new VcapClientResult();
        }

        public VcapClientResult BindService(string argProvisionedServiceName, string argAppName)
        {
            var apps = new AppsHelper(credMgr);

            Application app = apps.GetApplication(argAppName);
            app.Services.Add(argProvisionedServiceName);

            var request = new VcapJsonRequest(credMgr, Method.PUT, app, Constants.APPS_PATH, app.Name);
            RestResponse response = request.Execute();

            // Ruby code re-gets info
            app = apps.GetApplication(argAppName);
            if (app.Started)
            {
                apps.Restart(app);
            }
            return new VcapClientResult();
        }

        public void UnbindService(AppService appservice, Application application, Cloud cloud)
        {
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.PUT;
            request.Resource = "/apps/" + application.Name;
            request.AddHeader("Authorization", cloud.AccessToken);
            application.Services.Remove(appservice.Name);
            request.AddObject(application);
            request.RequestFormat = DataFormat.Json;
            client.Execute(request);
            var apps = new AppsHelper(token);
            apps.RestartApp(application, cloud);
        }
    }
}