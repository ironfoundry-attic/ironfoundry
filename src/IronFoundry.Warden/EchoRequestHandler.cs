using IronFoundry.WardenProtocol;

namespace IronFoundry.Warden
{
    public class EchoRequestHandler : RequestHandler
    {
        private readonly EchoRequest request;

        public EchoRequestHandler(Request request) : base(request)
        {
            this.request = (EchoRequest)request;
        }

        public override Response Handle()
        {
            return new EchoResponse { Message = request.Message };
        }
    }
}
