namespace IronFoundry.Vcap
{
    using System;
    using System.Collections.Generic;
    using IronFoundry.Properties;
    using IronFoundry.Types;
    using Newtonsoft.Json.Linq;
    using RestSharp;

    internal class UserHelper : BaseVmcHelper
    {
        public UserHelper(VcapUser proxyUser, VcapCredentialManager credMgr)
            : base(proxyUser, credMgr) { }

        public VcapClientResult Login(string argEmail, string argPassword)
        {
            VcapClientResult rv;

            VcapJsonRequest r = base.BuildVcapJsonRequest(Method.POST, Constants.USERS_PATH, argEmail, "tokens");
            r.AddBody(new { password = argPassword });
            RestResponse response = r.Execute();
            if (response.Content.IsNullOrEmpty())
            {
                rv = new VcapClientResult(false, Resources.Vmc_NoContentReturned_Text);
            }
            else
            {
                var parsed = JObject.Parse(response.Content);
                string token = parsed.Value<string>("token");
                credMgr.RegisterToken(token);
                rv = new VcapClientResult();
            }

            return rv;
        }

        public VcapClientResult ChangePassword(string user, string newpassword)
        {
            VcapRequest r = base.BuildVcapRequest(Constants.USERS_PATH, user);
            RestResponse response = r.Execute();

            JObject parsed = JObject.Parse(response.Content);
            parsed["password"] = newpassword;

            VcapJsonRequest put = base.BuildVcapJsonRequest(Method.PUT, Constants.USERS_PATH, user);
            put.AddBody(parsed);
            response = put.Execute();

            return new VcapClientResult();
        }

        public VcapClientResult AddUser(string email, string password)
        {
            VcapJsonRequest r = base.BuildVcapJsonRequest(Method.POST, Constants.USERS_PATH);
            r.AddBody(new { email = email, password = password });
            RestResponse response = r.Execute();
            return new VcapClientResult();
        }

        public VcapClientResult DeleteUser(string email)
        {
            var appsHelper = new AppsHelper(proxyUser, credMgr);
            foreach (Application a in appsHelper.GetApplications(email))
            {
                appsHelper.Delete(a.Name);
            }

            var servicesHelper = new ServicesHelper(proxyUser, credMgr);
            foreach (ProvisionedService ps in servicesHelper.GetProvisionedServices(email))
            {
                servicesHelper.DeleteService(ps.Name);
            }

            VcapJsonRequest r = base.BuildVcapJsonRequest(Method.DELETE, Constants.USERS_PATH, email);
            RestResponse response = r.Execute();
            return new VcapClientResult();
        }

        public VcapUser GetUser(string email)
        {
            VcapJsonRequest r = base.BuildVcapJsonRequest(Method.GET, Constants.USERS_PATH, email);
            return r.Execute<VcapUser>();
        }

        public IEnumerable<VcapUser> GetUsers()
        {
            VcapJsonRequest r = base.BuildVcapJsonRequest(Method.GET, Constants.USERS_PATH);
            return r.Execute<VcapUser[]>();
        }
    }
}