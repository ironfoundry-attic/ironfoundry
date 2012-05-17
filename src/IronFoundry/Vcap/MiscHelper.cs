namespace IronFoundry.Vcap
{
    using System;
    using IronFoundry.Types;

    internal class MiscHelper : BaseVmcHelper
    {
        public MiscHelper(VcapUser proxyUser, VcapCredentialManager credMgr)
            : base(proxyUser, credMgr) { }

        public VcapClientResult Info()
        {
            VcapRequest r = BuildVcapRequest(Constants.INFO_PATH);
            return new VcapClientResult(true, r.Execute<Info>());
        }

        internal VcapRequest BuildInfoRequest()
        {
            return BuildVcapRequest(Constants.INFO_PATH);
        }

        public VcapClientResult Target(Uri argUri = null)
        {
            VcapClientResult rv;

            if (null == argUri)
            {
                // Just return current target
                rv = new VcapClientResult(false, credMgr.CurrentTarget.AbsoluteUriTrimmed());
            }
            else
            {
                // "target" does the same thing as "info", but not logged in
                // considered valid if name, build, version and support are all non-null
                // without argument, displays current target
                VcapRequest r = base.BuildVcapRequest(false, argUri, Constants.INFO_PATH);
                Info info = r.Execute<Info>();

                bool success = false;
                if (null != info)
                {
                    success = false == info.Name.IsNullOrWhiteSpace() &&
                              false == info.Build.IsNullOrWhiteSpace() &&
                              false == info.Version.IsNullOrWhiteSpace() &&
                              false == info.Support.IsNullOrWhiteSpace();
                }

                if (success)
                {
                    credMgr.SetTarget(argUri);
                    credMgr.StoreTarget();
                }

                rv = new VcapClientResult(success, credMgr.CurrentTarget.AbsoluteUriTrimmed());
            }

            return rv;
        }
    }
}