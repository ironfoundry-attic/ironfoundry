namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class EchoRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly EchoRequest request;

        public EchoRequestHandler(Request request)
            : base(request)
        {
            this.request = (EchoRequest)request;
        }

        public override Response Handle()
        {
            log.Trace("Message: '{0}'", request.Message);
            return new EchoResponse { Message = request.Message };
        }
    }
}
