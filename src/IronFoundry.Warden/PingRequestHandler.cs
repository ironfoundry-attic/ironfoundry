using IronFoundry.WardenProtocol;

namespace IronFoundry.Warden
{
    public class PingRequestHandler : RequestHandler
    {
        private readonly PingRequest request;

        public PingRequestHandler(Request request) : base(request)
        {
            this.request = (PingRequest)request;
        }

        public override Response Handle()
        {
            return new PingResponse();
        }
    }
}
