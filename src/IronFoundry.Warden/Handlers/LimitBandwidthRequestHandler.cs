namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class LimitBandwidthRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly LimitBandwidthRequest request;

        public LimitBandwidthRequestHandler(Request request)
            : base(request)
        {
            this.request = (LimitBandwidthRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            log.Trace("Handle: '{0}' Burst: '{1}' Rate: '{2}'", request.Handle, request.Burst, request.Rate);
            return new LimitBandwidthResponse { Burst = 0, Rate = 0 };
        }
    }
}
