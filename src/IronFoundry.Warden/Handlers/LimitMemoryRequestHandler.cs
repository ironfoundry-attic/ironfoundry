namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class LimitMemoryRequestHandler : RequestHandler
    {
        private readonly LimitMemoryRequest request;

        public LimitMemoryRequestHandler(Request request)
            : base(request)
        {
            this.request = (LimitMemoryRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            return new LimitMemoryResponse { LimitInBytes = 134217728 }; // TODO 128 MB
        }
    }
}
