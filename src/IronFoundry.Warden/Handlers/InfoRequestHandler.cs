namespace IronFoundry.Warden.Handlers
{
    using System;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class InfoRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly InfoRequest request;
        private readonly InfoBuilder infoBuilder;

        public InfoRequestHandler(IContainerManager containerManager, Request request)
            : base(request)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            this.infoBuilder = new InfoBuilder(containerManager);
            this.request = (InfoRequest)request;
        }

        public override Task<Response> HandleAsync()
        {
            // TODO do work!
            log.Trace("Handle: '{0}'", request.Handle);
            return Task.FromResult<Response>(infoBuilder.GetInfoResponseFor(request.Handle));
        }
    }
}
