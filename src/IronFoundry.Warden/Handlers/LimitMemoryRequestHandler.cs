namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class LimitMemoryRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly LimitMemoryRequest request;

        public LimitMemoryRequestHandler(Request request)
            : base(request)
        {
            this.request = (LimitMemoryRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            log.Trace("Handle: '{0}' LimitInBytes: '{1}'", request.Handle, request.LimitInBytes);
            return new LimitMemoryResponse { LimitInBytes = 134217728 }; // TODO 128 MB
        }
    }
}
