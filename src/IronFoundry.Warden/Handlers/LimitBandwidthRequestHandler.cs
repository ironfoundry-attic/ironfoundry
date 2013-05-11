namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class LimitBandwidthRequestHandler : RequestHandler
    {
        private readonly LimitBandwidthRequest request;

        public LimitBandwidthRequestHandler(Request request)
            : base(request)
        {
            this.request = (LimitBandwidthRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            return new LimitBandwidthResponse { Burst = 0, Rate = 0 };
        }
    }
}
