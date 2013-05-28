namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class CopyInRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly CopyInRequest request;

        public CopyInRequestHandler(Request request)
            : base(request)
        {
            this.request = (CopyInRequest)request;
        }

        public override Response Handle()
        {
            // TODO: do work!
            log.Trace("SrcPath: '{0}' DstPath: '{1}'", request.SrcPath, request.DstPath);
            return new CopyInResponse();
        }
    }
}
