using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using CloudFoundry.Net.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CloudFoundry.Net.Vmc
{
    internal class VmcAdministration
    {
        public string Login(string username, string pass, string url)
        {
            var client = new RestClient();
            client.BaseUrl = url;
            var request = new RestRequest();
            request.Method = Method.POST;
            request.Resource = "/users/" + username + "/tokens";
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new { password = pass });
            var content = client.Execute(request).Content;
            return content;
        }

        public Cloud Login(Cloud currentcloud)
        {
            var client = new RestClient();
            var request = new RestRequest();
            client.BaseUrl = currentcloud.Url;
            request.Method = Method.POST;
            request.RequestFormat = DataFormat.Json;
            request.Resource = "/users/" + currentcloud.Email + "/tokens";
            request.AddBody(new { password = currentcloud.Password });
            var response = client.Execute(request).Content;
            JObject jobj = JObject.Parse(response);
            currentcloud.AccessToken = jobj.Value<string>("token");
            return currentcloud;
        }

        bool ChangePassword(string username, string newpassword, string url, string accesstoken)
        {
            var client = new RestClient();
            client.BaseUrl = url;
            var request = new RestRequest();
            request.Method = Method.PUT;
            request.Resource = "/users/" + username + "";
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Authorization", accesstoken);
            request.AddBody(new { email = username, password = newpassword });
            if (client.Execute(request).StatusCode == System.Net.HttpStatusCode.NoContent)
                return false;
            else
                return true;
        }

        bool ChangePassword(Cloud currentcloud)
        {
            var client = new RestClient();
            client.BaseUrl = currentcloud.Url;
            var request = new RestRequest();
            request.Method = Method.PUT;
            request.Resource = "/users/" + currentcloud.Email + "";
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Authorization", currentcloud.AccessToken);
            request.AddBody(new { email = currentcloud.Email, password = currentcloud.Password });
            if (client.Execute(request).StatusCode == System.Net.HttpStatusCode.NoContent)
                return false;
            else
                return true;
        }


    }
}
