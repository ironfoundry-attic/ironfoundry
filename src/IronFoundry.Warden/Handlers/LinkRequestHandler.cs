namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class LinkRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly LinkRequest request;

        public LinkRequestHandler(Request request)
            : base(request)
        {
            this.request = (LinkRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            log.Trace("Handle: '{0}' JobId: '{1}'", request.Handle, request.JobId);
            return new LinkResponse { ExitStatus = 0 };
        }
    }
}
