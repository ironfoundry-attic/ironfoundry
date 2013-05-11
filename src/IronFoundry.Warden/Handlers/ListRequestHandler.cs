namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class ListRequestHandler : RequestHandler
    {
        private readonly ListRequest request;

        public ListRequestHandler(Request request)
            : base(request)
        {
            this.request = (ListRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            return new ListResponse();
        }
    }
}
