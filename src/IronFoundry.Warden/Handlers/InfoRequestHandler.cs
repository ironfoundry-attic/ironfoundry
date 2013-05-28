namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Misc;
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class InfoRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly InfoRequest request;

        public InfoRequestHandler(Request request)
            : base(request)
        {
            this.request = (InfoRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            log.Trace("Handle: '{0}'", request.Handle);
            var hostIp = Utility.GetLocalIPAddress().ToString();
            return new InfoResponse(hostIp, "10.0.0.1", "C:/IronFoundry/warden/app1");
        }
    }
}
