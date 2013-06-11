namespace IronFoundry.Warden.Handlers
{
    using System;
    using System.Threading.Tasks;
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

        public override Task<Response> HandleAsync()
        {
            var factory = new ContainerFactory(request.Rootfs); // TODO: use something other than rootfs
            Container c = factory.CreateContainer();
            containerManager.AddContainer(c);
            return Task.FromResult<Response>(new CreateResponse { Handle = c.Handle });
        }
    }
}
