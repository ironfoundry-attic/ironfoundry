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
        private string GetServices(string url, string accesstoken)
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

        private string GetServices(Cloud currentcloud)
        {
            var client = new RestClient();
            client.BaseUrl = currentcloud.Url;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = "/info/services";
            request.AddHeader("Authorization", currentcloud.AccessToken);
            return client.Execute(request).Content;
      
        }
        public List<SystemService> GetAvailableServices(Cloud cloud) 
        {
            //Get /info/services
            return (List<SystemService>)JsonConvert.DeserializeObject(GetServices(cloud), typeof(List<SystemService>)); 

        }

        public List<AppService> GetProvisionedServices(Cloud cloud)
        {
            //Get /services
            return (List<AppService>)JsonConvert.DeserializeObject(GetServices(cloud), typeof(List<AppService>));
        }


    }
}
