namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class LimitDiskRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly LimitDiskRequest request;

        public LimitDiskRequestHandler(Request request)
            : base(request)
        {
            this.request = (LimitDiskRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            log.Trace("Handle: '{0}' Block: '{1}' Byte: '{2}' Inode: '{3}'", request.Handle, request.Block, request.Byte, request.Inode);
            return new LimitDiskResponse();
        }
    }
}
