using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using RestSharp;

namespace CloudFoundry.Net.Vmc
{
    internal class VmcInit
    {
        public string Info (string accesstoken, string url){
            if (url == null){
                return ("Target URL has to be set");
            } else {
                var client = new RestClient();
                client.BaseUrl = url;
                var request = new RestRequest();
                request.Resource = "/info";
                if (accesstoken != null)
                    request.AddHeader("Authorization", accesstoken);
                return client.Execute(request).Content;
            }
        }
    }
}
