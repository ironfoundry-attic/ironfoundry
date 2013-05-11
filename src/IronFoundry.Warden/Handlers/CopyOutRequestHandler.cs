namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class CopyOutRequestHandler : RequestHandler
    {
        private readonly CopyOutRequest request;

        public CopyOutRequestHandler(Request request)
            : base(request)
        {
            this.request = (CopyOutRequest)request;
        }

        public override Response Handle()
        {
            // TODO: do work!
            return new CopyOutResponse();
        }
    }
}
