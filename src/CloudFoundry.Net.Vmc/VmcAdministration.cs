using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;

namespace CloudFoundry.Net.Vmc
{
    internal class VmcAdministration
    {
       public string Login (string username, string pass, string url){
            var client = new RestClient();
            client.BaseUrl = url;
            var request = new RestRequest();
            request.Method = Method.POST;
            request.Resource = "/users/"+username+"/tokens";
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new { password = pass });
            var content = client.Execute(request).Content;
            return content;
        }       

        void ClearAuthToken () {

        }
    }
}
