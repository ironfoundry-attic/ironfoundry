namespace IronFoundry.Warden.Handlers
{
    using System;
    using Containers;
    using Protocol;

    public abstract class ContainerRequestHandler : RequestHandler
    {
        private readonly IContainerManager containerManager;
        private readonly IContainerRequest containerRequest;

        public ContainerRequestHandler(IContainerManager containerManager, Request request)
            : base(request)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            this.containerManager = containerManager;

            this.containerRequest = (IContainerRequest)request;
        }

        protected Container GetContainer()
        {
            return containerManager.GetContainer(containerRequest.Handle);
        }
    }
}
