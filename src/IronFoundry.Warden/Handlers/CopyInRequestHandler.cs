namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class CopyInRequestHandler : RequestHandler
    {
        private readonly CopyInRequest request;

        public CopyInRequestHandler(Request request)
            : base(request)
        {
            this.request = (CopyInRequest)request;
        }

        public override Response Handle()
        {
            // TODO: do work!
            return new CopyInResponse();
        }
    }
}
