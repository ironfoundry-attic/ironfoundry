namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class StreamRequestHandler : RequestHandler
    {
        private readonly StreamRequest request;

        public StreamRequestHandler(Request request)
            : base(request)
        {
            this.request = (StreamRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            return new StreamResponse();
        }
    }
}
