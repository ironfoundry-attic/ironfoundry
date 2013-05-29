namespace IronFoundry.Warden.Handlers
{
    using System;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class LinkRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly LinkRequest request;
        private readonly InfoBuilder infoBuilder;

        public LinkRequestHandler(IContainerManager containerManager, Request request)
            : base(request)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            this.infoBuilder = new InfoBuilder(containerManager);
            this.request = (LinkRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            log.Trace("Handle: '{0}' JobId: '{1}'", request.Handle, request.JobId);
            return new LinkResponse
            {
                ExitStatus = 0,
                Stderr = "TODO STDERR",
                Stdout = "TODO STDOUT",
                Info = infoBuilder.GetInfoResponseFor(request.Handle)
            };
        }
    }
}
