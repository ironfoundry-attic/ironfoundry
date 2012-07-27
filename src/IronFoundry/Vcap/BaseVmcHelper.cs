namespace IronFoundry.Vcap
{
    using System;
    using System.Collections.Generic;
    using IronFoundry.Types;
    using Newtonsoft.Json;
    using RestSharp;

    internal abstract class BaseVmcHelper
    {
        protected readonly VcapCredentialManager credMgr;
        protected readonly VcapUser proxyUser;

        public BaseVmcHelper(VcapUser proxyUser, VcapCredentialManager credMgr)
        {
            this.proxyUser = proxyUser;
            this.credMgr = credMgr;
        }

        public string GetApplicationJson(string name)
        {
            VcapRequest r = BuildVcapRequest(Constants.APPS_PATH, name);
            return r.Execute().Content;
        }

        public Application GetApplication(string name)
        {
            string json = GetApplicationJson(name);
            return JsonConvert.DeserializeObject<Application>(json);
        }

        public IEnumerable<Application> GetApplications(string proxy_user = null)
        {
            VcapRequest r = BuildVcapRequest(Constants.APPS_PATH);
            return r.Execute<Application[]>();
        }

        protected bool AppExists(string name)
        {
            bool rv = true;
            try
            {
                string appJson = GetApplicationJson(name);
            }
            catch (VcapNotFoundException)
            {
                rv = false;
            }
            return rv;
        }

        protected VcapRequest BuildVcapRequest(params object[] resourceParams)
        {
            return new VcapRequest(ProxyUserEmail, credMgr, resourceParams);
        }

        protected VcapRequest BuildVcapRequest(bool useAuth, Uri uri, params object[] resourceParams)
        {
            return new VcapRequest(ProxyUserEmail, credMgr, useAuth, uri, resourceParams);
        }

        protected VcapRequest BuildVcapRequest(Method method, params string[] resourceParams)
        {
            return new VcapRequest(ProxyUserEmail, credMgr, method, resourceParams);
        }

        protected VcapJsonRequest BuildVcapJsonRequest(Method method, params string[] resourceParams)
        {
            return new VcapJsonRequest(ProxyUserEmail, credMgr, method, resourceParams);
        }

        private string ProxyUserEmail
        {
            get
            {
                string proxyUserEmail = null;
                if (null != proxyUser)
                {
                    proxyUserEmail = proxyUser.Email;
                }
                return proxyUserEmail;
            }
        }
    }
}