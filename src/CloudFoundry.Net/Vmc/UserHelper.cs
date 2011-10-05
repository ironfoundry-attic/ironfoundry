namespace CloudFoundry.Net.Vmc
{
    using Newtonsoft.Json.Linq;
    using Properties;
    using RestSharp;

    internal class UserHelper : BaseVmcHelper
    {
        public UserHelper(VcapCredentialManager argCredentialManager)
            : base(argCredentialManager) { }

        public VcapClientResult Login(string argEmail, string argPassword)
        {
            VcapClientResult rv;

            RestClient client = buildClient(false);

            RestRequest request = buildRequest(Method.POST, DataFormat.Json, Constants.USERS_PATH, argEmail, "tokens");
            request.AddBody(new { password = argPassword });

            RestResponse response = executeRequest(client, request);
            if (response.Content.IsNullOrEmpty())
            {
                rv = new VcapClientResult(false, Resources.Vmc_NoContentReturned_Text);
            }
            else
            {
                var parsed = JObject.Parse(response.Content);
                string token = parsed.Value<string>("token");
                credentialManager.RegisterToken(token);
                rv = new VcapClientResult();
            }

            return rv;
        }
    }
}