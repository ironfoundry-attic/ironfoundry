namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class CopyOutRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly CopyOutRequest request;

        public CopyOutRequestHandler(Request request)
            : base(request)
        {
            this.request = (CopyOutRequest)request;
        }

        public override Response Handle()
        {
            // TODO: do work!
            log.Trace("SrcPath: '{0}' DstPath: '{1}'", request.SrcPath, request.DstPath);
            return new CopyOutResponse();
        }
    }
}
