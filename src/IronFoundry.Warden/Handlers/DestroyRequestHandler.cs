namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class DestroyRequestHandler : RequestHandler
    {
        private readonly DestroyRequest request;

        public DestroyRequestHandler(Request request)
            : base(request)
        {
            this.request = (DestroyRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            return new DestroyResponse();
        }
    }
}
