using IronFoundry.Warden.Protocol;

namespace IronFoundry.Warden.Handlers
{
    public class PingRequestHandler : RequestHandler
    {
        private readonly PingRequest request;

        public PingRequestHandler(Request request)
            : base(request)
        {
            this.request = (PingRequest)request;
        }

        public override Response Handle()
        {
            return new PingResponse();
        }
    }
}
