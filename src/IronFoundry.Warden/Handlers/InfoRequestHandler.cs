namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Misc;
    using IronFoundry.Warden.Protocol;

    public class InfoRequestHandler : RequestHandler
    {
        private readonly InfoRequest request;

        public InfoRequestHandler(Request request)
            : base(request)
        {
            this.request = (InfoRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            var hostIp = Utility.GetLocalIPAddress().ToString();
            return new InfoResponse(hostIp, "10.0.0.1", "C:/IronFoundry/warden/app1");
        }
    }
}
