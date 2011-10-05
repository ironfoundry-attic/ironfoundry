namespace CloudFoundry.Net.Vmc
{
    using System.Collections.Generic;
    using CloudFoundry.Net.Types;
    using Newtonsoft.Json;
    using RestSharp;

    internal class ServicesHelper : BaseVmcHelper
    {
        public ServicesHelper(VcapCredentialManager argCredentialManager)
            : base(argCredentialManager) { }

        public IEnumerable<SystemService> GetSystemServices()
        {
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.GET, Constants.GLOBAL_SERVICES_PATH);
            RestResponse response = executeRequest(client, request);

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
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.GET, Constants.SERVICES_PATH);
            return executeRequest<ProvisionedService[]>(client, request);
        }

        public VcapClientResult BindService(string argProvisionedServiceName, string argAppName)
        {
            var apps = new AppsHelper(credentialManager);

            Application app = apps.GetApplication(argAppName);
            app.Services.Add(argProvisionedServiceName);
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.PUT, DataFormat.Json, Constants.APPS_PATH, app.Name);
            request.AddBody(app);
            RestResponse response = executeRequest(client, request);

            // Ruby code re-gets info
            app = apps.GetApplication(argAppName);
            if (app.Started)
            {
                apps.Restart(app);
            }
            return new VcapClientResult();
        }

#if UNUSED
        public void CreateService(AppService appservice, Cloud cloud) {
            /*
             *"type":"database","tier":"free","vendor":"mysql","version":"5.1","name":"mysql-870f3"
             *
             */
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.POST;
            request.Resource = "/services";
            request.AddHeader("Authorization", cloud.AccessToken);
            request.AddObject(appservice);
            request.RequestFormat = DataFormat.Json;
            client.Execute(request);

        public void DeleteService (AppService appservice, Cloud cloud) {
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.DELETE;
            request.Resource = "/services/" + appservice.Name;
            request.AddHeader("Authorization", cloud.AccessToken);
            client.Execute(request);
            //should prolly put a try-catch in here to catch the exception when the service is not in the current running list
        }

        public void UnbindService (AppService appservice, Application application, Cloud cloud) {
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
#endif
    }
}