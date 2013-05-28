namespace IronFoundry.Warden.Handlers
{
    using System;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class DestroyRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IContainerManager containerManager;
        private readonly DestroyRequest request;

        public DestroyRequestHandler(IContainerManager containerManager, Request request)
            : base(request)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            this.containerManager = containerManager;
            this.request = (DestroyRequest)request;
        }

        public override Response Handle()
        {
            if (request.Handle.IsNullOrWhiteSpace())
            {
                throw new WardenException("Container handle is required.");
            }
            else
            {
                log.Trace("Handle: '{0}'", request.Handle);
                containerManager.DestroyContainer(new ContainerHandle(request.Handle));
            }
            return new DestroyResponse();
        }
    }
}
