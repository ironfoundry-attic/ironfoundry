namespace IronFoundry.Warden.Handlers
{
    using System;
    using System.Linq;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Protocol;

    public class ListRequestHandler : RequestHandler
    {
        private readonly IContainerManager containerManager;
        private readonly ListRequest request;

        public ListRequestHandler(IContainerManager containerManager, Request request)
            : base(request)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            this.containerManager = containerManager;
            this.request = (ListRequest)request;
        }

        public override Response Handle()
        {
            var response =  new ListResponse();
            response.Handles.AddRange(containerManager.Handles.Select(h => (string)h));
            return response;
        }
    }
}
