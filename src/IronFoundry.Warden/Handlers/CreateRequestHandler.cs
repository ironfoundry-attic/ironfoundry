namespace IronFoundry.Warden.Handlers
{
    using System;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Protocol;

    public class CreateRequestHandler : RequestHandler
    {
        private readonly IContainerManager containerManager;
        private readonly CreateRequest request;

        public CreateRequestHandler(IContainerManager containerManager, Request request)
            : base(request)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            this.containerManager = containerManager;
            this.request = (CreateRequest)request;
        }

        public override Response Handle()
        {
            var factory = new ContainerFactory(request.Rootfs); // TODO: use something other than rootfs
            Container c = factory.CreateContainer();
            containerManager.AddContainer(c);
            return new CreateResponse { Handle = c.Handle };
        }
    }
}
