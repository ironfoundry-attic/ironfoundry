namespace IronFoundry.Vcap
{
    using Models;

    public class VcapClientResult
    {
        private readonly bool success = true;
        private readonly string message;
        private readonly VcapResponse vcapResponse;
        private readonly Message responseMessage;

        public VcapClientResult()
        {
            success = true;
        }

        public VcapClientResult(bool argSuccess)
        {
            success = argSuccess;
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
    }
}
