namespace CloudFoundry.Net.Vmc
{
    using System;
    using RestSharp;
    using Types;

    internal class MiscHelper : BaseVmcHelper
    {
        public MiscHelper(VcapCredentialManager argCredentialManager)
            : base(argCredentialManager) { }

        public VcapClientResult Info()
        {
            RestClient client = BuildClient();
            RestRequest request = BuildRequest(Method.GET, Constants.INFO_PATH);
            Info info = ExecuteRequest<Info>(client, request);
            return new VcapClientResult(true, info);
        }

        public VcapClientResult Target(Uri argUri = null)
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
                RestClient client = BuildClient(false, argUri);
                RestRequest request = BuildRequest(Method.GET, Constants.INFO_PATH);
                var info = ExecuteRequest<Info>(client, request);
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