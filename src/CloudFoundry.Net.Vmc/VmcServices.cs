using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using CloudFoundry.Net.Types;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Vmc
{
    internal class VmcServices
    {
        public string GetServices(string url, string accesstoken)
        {
            if (url == null)
            {
                return ("Target URL has to be set");
            }
            else if (accesstoken == null)
            {
                return ("Please login first");
            }
            else
            {
                var client = new RestClient();
                client.BaseUrl = url;
                var request = new RestRequest();
                request.Method = Method.GET;
                request.Resource = "/info/services";
                request.AddHeader("Authorization", accesstoken);
                return client.Execute(request).Content;
            }
        }

        
        public List<SystemServices> GetAvailableServices(Cloud cloud) 
        {
            List<SystemServices> datastores = new List<SystemServices>();
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = "/info/services";
            request.AddHeader("Authorization", cloud.AccessToken);
            string response = client.Execute(request).Content;
            var list = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, SystemServices>>>>(response);
            foreach (var val in list.Values)
                foreach (var val1 in val.Values)
                    foreach (var val2 in val1.Values)
                        datastores.Add(val2);
            return datastores; 
        }

        public List<AppService> GetProvisionedServices(Cloud cloud)
        {            
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = "/services";
            request.AddHeader("Authorization", cloud.AccessToken);
            string response = client.Execute(request).Content;
            var list = JsonConvert.DeserializeObject<List<AppService>>(response);
            return list;
        }

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
            
        }

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

        public void BindService (AppService appservice, Application application, Cloud cloud) {

            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.PUT;
            request.Resource = "/apps/"+application.Name;
            request.AddHeader("Authorization", cloud.AccessToken);
            application.Services.Add(appservice.Name);
            request.AddObject(application);
            request.RequestFormat = DataFormat.Json;
            client.Execute(request);
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
        }
    }
}
