using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronFoundry.Vcap
{
    using Models;
    using Newtonsoft.Json;
    using RestSharp;
    using Types;

    internal abstract class BaseVmcHelper
    {
        protected readonly VcapCredentialManager CredentialManager;
        protected readonly VcapUser ProxyUser;

        public BaseVmcHelper(VcapUser proxyUser, VcapCredentialManager credentialManager)
        {
            ProxyUser = proxyUser;
            CredentialManager = credentialManager;
        }

        private string ProxyUserEmail
        {
            get
            {
                string proxyUserEmail = null;
                if (null != ProxyUser)
                {
                    proxyUserEmail = ProxyUser.Email;
                }
                return proxyUserEmail;
            }
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
            return new VcapRequest(ProxyUserEmail, CredentialManager, resourceParams);
        }

        protected VcapRequest BuildVcapRequest(bool useAuth, Uri uri, params object[] resourceParams)
        {
            return new VcapRequest(ProxyUserEmail, CredentialManager, useAuth, uri, resourceParams);
        }

        protected VcapRequest BuildVcapRequest(Method method, params string[] resourceParams)
        {
            return new VcapRequest(ProxyUserEmail, CredentialManager, method, resourceParams);
        }

        protected VcapJsonRequest BuildVcapJsonRequest(Method method, params string[] resourceParams)
        {
            return new VcapJsonRequest(ProxyUserEmail, CredentialManager, method, resourceParams);
        }
    }
}