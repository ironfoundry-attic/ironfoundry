using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;

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
    }
}
