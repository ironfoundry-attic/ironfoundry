namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class NetInRequestHandler : RequestHandler
    {
        private readonly NetInRequest request;

        public NetInRequestHandler(Request request)
            : base(request)
        {
            this.request = (NetInRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            return new NetInResponse();
        }
    }
}
