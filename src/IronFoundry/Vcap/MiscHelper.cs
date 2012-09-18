namespace IronFoundry.Vcap
{
    using System;
    using IronFoundry.Types;
    using Models;

    internal class MiscHelper : BaseVmcHelper
    {
        public MiscHelper(VcapUser proxyUser, VcapCredentialManager credentialManager)
            : base(proxyUser, credentialManager) { }

        public Info GetInfo()
        {
            VcapRequest r = BuildVcapRequest(Constants.INFO_PATH);
            return r.Execute<Info>();
        }

        internal VcapRequest BuildInfoRequest()
        {
            return BuildVcapRequest(Constants.INFO_PATH);
        }

        public void Target(Uri uri)
        {
            // "target" does the same thing as "info", but not logged in
            // considered valid if name, build, version and support are all non-null
            VcapRequest request = BuildVcapRequest(false, uri, Constants.INFO_PATH);
            Info info = request.Execute<Info>();

            var success = info != null &&
                !info.Name.IsNullOrWhiteSpace() &&
                !info.Build.IsNullOrWhiteSpace() &&
                !info.Version.IsNullOrWhiteSpace() &&
                !info.Support.IsNullOrWhiteSpace();

            if (success)
            {
                CredentialManager.SetTarget(uri);
                CredentialManager.StoreTarget();
            }
            else
            {
                throw new VcapTargetException(request.ErrorMessage);
            }
        }
    }
}