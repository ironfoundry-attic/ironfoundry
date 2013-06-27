namespace IronFoundry.Warden.Handlers
{
    using System.Threading.Tasks;
    using Containers;
    using Protocol;

    public class CreateRequestHandler : ContainerRequestHandler
    {
        private readonly CreateRequest request;

        public CreateRequestHandler(IContainerManager containerManager, Request request)
            : base(containerManager, request)
        {
            this.request = (CreateRequest)request;
        }

        public override Task<Response> HandleAsync()
        {
            return Task.Run<Response>(() =>
                {
                    // before

                    // do
                    var container = new Container();
                    containerManager.AddContainer(container);

                    // after
                    container.AfterCreate();
                    return new CreateResponse { Handle = container.Handle };
                });
        }
    }
}
