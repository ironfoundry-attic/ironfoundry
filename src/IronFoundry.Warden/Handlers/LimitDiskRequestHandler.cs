namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class LimitDiskRequestHandler : RequestHandler
    {
        private readonly LimitDiskRequest request;

        public LimitDiskRequestHandler(Request request)
            : base(request)
        {
            this.request = (LimitDiskRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            return new LimitDiskResponse();
        }
    }
}
