namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class StopRequestHandler : RequestHandler
    {
        private readonly StopRequest request;

        public StopRequestHandler(Request request)
            : base(request)
        {
            this.request = (StopRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            return new StopResponse();
        }
    }
}
