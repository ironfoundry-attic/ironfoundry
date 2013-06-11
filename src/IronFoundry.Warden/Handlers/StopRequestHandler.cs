namespace IronFoundry.Warden.Handlers
{
    using System.Threading.Tasks;
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class StopRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly StopRequest request;

        public StopRequestHandler(Request request)
            : base(request)
        {
            this.request = (StopRequest)request;
        }

        public override Task<Response> HandleAsync()
        {
            // TODO do work!
            log.Trace("Handle: '{0}' Background: '{1}' Kill: '{2}'", request.Handle, request.Background, request.Kill);
            return Task.FromResult<Response>(new StopResponse());
        }
    }
}
