namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class SpawnRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly SpawnRequest request;

        public SpawnRequestHandler(Request request)
            : base(request)
        {
            this.request = (SpawnRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work
            log.Trace("Handle: '{0}' Script: '{1}'", request.Handle, request.Script);
            return new SpawnResponse { JobId = 1 };
        }
    }
}
