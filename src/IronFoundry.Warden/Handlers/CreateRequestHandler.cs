namespace IronFoundry.Warden.Handlers
{
    using System;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Protocol;

    public class CreateRequestHandler : RequestHandler
    {
        private readonly CreateRequest request;

        public CreateRequestHandler(Request request)
            : base(request)
        {
            this.request = (CreateRequest)request;
        }

        public override Response Handle()
        {
            var factory = new ContainerFactory(request.Rootfs); // TODO: use something other than rootfs
            Container c = factory.CreateContainer();
            return new CreateResponse { Handle = c.Handle };
        }
    }
}
