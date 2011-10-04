namespace CloudFoundry.Net.Vmc
{
    using System;
    using RestSharp;
    using Types;

    public class MiscHelper : BaseVmcHelper
    {
        private readonly VcapCredentialManager credentialManager = new VcapCredentialManager();

        public VcapClientResult Info()
        {
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.GET, Constants.INFO_PATH);
            Info info = executeRequest<Info>(client, request);
            return new VcapClientResult(true, info);
        }

        public VcapClientResult Target(Uri argUri)
        {
            VcapClientResult rv;

            if (null == argUri)
            {
                // Just return current target
                rv = new VcapClientResult(false, credentialManager.CurrentTarget.AbsoluteUriTrimmed());
            }
            else
            {
                // "target" does the same thing as "info", but not logged in
                // considered valid if name, build, version and support are all non-null
                // without argument, displays current target
                RestClient client = buildClientNoAuth(argUri);
                RestRequest request = buildRequest(Method.GET, Constants.INFO_PATH);
                var info = executeRequest<Info>(client, request);
                bool success = false == info.Name.IsNullOrWhiteSpace() &&
                               false == info.Build.IsNullOrWhiteSpace() &&
                               false == info.Version.IsNullOrWhiteSpace() &&
                               false == info.Support.IsNullOrWhiteSpace();

                if (success)
                {
                    credentialManager.SetTarget(argUri);
                    credentialManager.StoreTarget();
                }

                rv = new VcapClientResult(success, credentialManager.CurrentTarget.AbsoluteUriTrimmed());
            }

            return rv;
        }
    }
}