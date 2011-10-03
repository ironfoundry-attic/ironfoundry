/*
namespace CloudFoundry.Net.Vmc
{
    using CloudFoundry.Net.Types;
    using Newtonsoft.Json.Linq;
    using RestSharp;

    internal class VmcAdministration
    {
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
*/