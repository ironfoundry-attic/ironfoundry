namespace IronFoundry.Warden.Handlers
{
    using System.Threading.Tasks;
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class NetInRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly NetInRequest request;

        public NetInRequestHandler(Request request)
            : base(request)
        {
            this.request = (NetInRequest)request;
        }

        public override Task<Response> HandleAsync()
        {
            // TODO do work!
            log.Trace("Handle: '{0}' ContainerPort: '{1}' HostPort: '{2}'", request.Handle, request.ContainerPort, request.HostPort);
            return Task.FromResult<Response>(new NetInResponse());
        }
    }
}
