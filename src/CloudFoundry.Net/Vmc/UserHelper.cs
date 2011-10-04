namespace CloudFoundry.Net.Vmc
{
    using System;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using Types;

    public class UserHelper : BaseVmcHelper
    {
        private readonly VcapCredentialManager credentialManager = new VcapCredentialManager();

        public VcapClientResult Login(Cloud argCloud)
        {
            return Login(new Uri(argCloud.Url), argCloud.Email, argCloud.Password);
        }

        public VcapClientResult Login(string argEmail, string argPassword)
        {
            return Login(credentialManager.CurrentTarget, argEmail, argPassword);
        }

        public VcapClientResult Login(Uri argUri, string argEmail, string argPassword)
        {
            RestClient client = buildClient(argUri);

            RestRequest request = buildRequest(Method.POST, DataFormat.Json, Constants.USERS_PATH, argEmail, "tokens");
            request.AddBody(new { password = argPassword });

            RestResponse response = executeRequest(client, request);

            var parsed = JObject.Parse(response.Content);

            string token = parsed.Value<string>("token");

            credentialManager.RegisterFor(argUri, token);

            return new VcapClientResult();
        }
    }
}