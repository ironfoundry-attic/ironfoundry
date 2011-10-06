namespace CloudFoundry.Net.Vmc
{
    internal abstract class BaseVmcHelper
    {
        protected readonly VcapCredentialManager credMgr;

        public BaseVmcHelper(VcapCredentialManager credMgr)
        {
            this.credMgr = credMgr;
        }
    }
}