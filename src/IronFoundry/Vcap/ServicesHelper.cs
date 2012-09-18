namespace IronFoundry.Vcap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;

    internal class ServicesHelper : BaseVmcHelper
    {
        public ServicesHelper(VcapUser proxyUser, VcapCredentialManager credentialManager)
            : base(proxyUser, credentialManager) { }

        public IEnumerable<SystemService> GetSystemServices()
        {
            VcapRequest r = BuildVcapRequest(Constants.GLOBAL_SERVICES_PATH);
            IRestResponse response = r.Execute();

            var list = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, SystemService>>>>(response.Content);

            var dataStores = from val in list.Values
                             from val1 in val.Values
                             from val2 in val1.Values
                             select val2;
            
            return dataStores.ToList(); 
        }

        public IEnumerable<ProvisionedService> GetProvisionedServices()
        {            
            VcapRequest r = BuildVcapRequest(Constants.SERVICES_PATH);
            return r.Execute<ProvisionedService[]>();
        }

        public void CreateService(string serviceName, string provisionedServiceName)
        {
            var services = GetSystemServices();
            var service = services.FirstOrDefault(s => s.Vendor == serviceName);
            if (service != null)
            {
                // from vmc client.rb
                var data = new
                {
                    name    = provisionedServiceName,
                    type    = service.Type,
                    tier    = "free",
                    vendor  = service.Vendor,
                    version = service.Version,
                };
                var r = BuildVcapJsonRequest(Method.POST, Constants.SERVICES_PATH);
                r.AddBody(data);
                r.Execute();
            }
        }

        public void DeleteService(string provisionedServiceName)
        {
            var request = BuildVcapJsonRequest(Method.DELETE, Constants.SERVICES_PATH, provisionedServiceName);
            request.Execute();
        }

        public void BindService(string provisionedServiceName, string appName)
        {
            var apps = new AppsHelper(ProxyUser, CredentialManager);

            Application app = apps.GetApplication(appName);
            app.Services.Add(provisionedServiceName);

            var request = BuildVcapJsonRequest(Method.PUT, Constants.APPS_PATH, app.Name);
            request.AddBody(app);
            request.Execute();

            // Ruby code re-gets info
            app = apps.GetApplication(appName);
            if (app.IsStarted)
            {
                apps.Restart(app);
            }
        }

        public void UnbindService(string provisionedServiceName, string appName)
        {
            var apps = new AppsHelper(ProxyUser, CredentialManager);
            string appJson = apps.GetApplicationJson(appName);
            var appParsed = JObject.Parse(appJson);
            var services = (JArray)appParsed["services"];
            appParsed["services"] = new JArray(services.Where(s => ((string)s) != provisionedServiceName));

            var r = BuildVcapJsonRequest(Method.PUT, Constants.APPS_PATH, appName);
            r.AddBody(appParsed);
            r.Execute();

            apps = new AppsHelper(ProxyUser, CredentialManager);
            apps.Restart(appName);
        }
    }
}
