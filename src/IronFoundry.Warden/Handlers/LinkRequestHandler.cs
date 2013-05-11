namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class LinkRequestHandler : RequestHandler
    {
        private readonly LinkRequest request;

        public LinkRequestHandler(Request request)
            : base(request)
        {
            this.request = (LinkRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            return new LinkResponse { ExitStatus = 0 };
        }
    }
}
