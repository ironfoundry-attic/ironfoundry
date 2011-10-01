namespace CloudFoundry.Net.Vmc
{
    using Types;

    public class VcapClientResult
    {
        private readonly bool success = true;
        private readonly string message;
        private readonly VcapResponse response;
        private readonly Cloud cloud; // TODO

        public VcapClientResult()
        {
            success = true;
        }

        public VcapClientResult(bool argSuccess, string argMessage)
        {
            success = argSuccess;
            message = argMessage;
        }

        public VcapClientResult(bool argSuccess, VcapResponse argResponse)
        {
            success = argSuccess;
            response = argResponse;
        }

        public VcapClientResult(Cloud argCloud)
        {
            cloud = argCloud;
        }

        public Cloud Cloud // TODO should not be here??
        {
            get { return cloud; }
        }

        public bool Success
        {
            get { return success; }
        }

        public string Message
        {
            get
            {
                string rv;

                if (null == response)
                {
                    rv = message;
                }
                else
                {
                    rv = response.Description; // TODO
                }

                return rv;
            }
        }
    }
}