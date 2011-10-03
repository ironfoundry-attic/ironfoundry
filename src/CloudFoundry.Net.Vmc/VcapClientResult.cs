namespace CloudFoundry.Net.Vmc
{
    using Types;

    public class VcapClientResult
    {
        private readonly bool success = true;
        private readonly string message;
        private readonly VcapResponse vcapResponse;
        private readonly Message responseMessage;
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
            vcapResponse = argResponse;
        }

        public VcapClientResult(bool argSuccess, Message argResponseMessage)
        {
            success = argSuccess;
            responseMessage = argResponseMessage;
        }

        public VcapClientResult(Cloud argCloud)
        {
            cloud = argCloud;
        }

        public bool Success
        {
            get { return success; }
        }

        public T GetResponseMessage<T>() where T: Message
        {
            return (T)responseMessage;
        }

        public string Message
        {
            get
            {
                string rv;

                if (null == vcapResponse)
                {
                    rv = message;
                }
                else
                {
                    rv = vcapResponse.Description; // TODO
                }

                return rv;
            }
        }

        public Cloud Cloud // TODO should not be here??
        {
            get { return cloud; }
        }
    }
}