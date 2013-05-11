namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class NetOutRequestHandler : RequestHandler
    {
        private readonly NetOutRequest request;

        public NetOutRequestHandler(Request request)
            : base(request)
        {
            this.request = (NetOutRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            return new NetOutResponse();
        }
    }
}
