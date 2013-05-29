namespace IronFoundry.Warden.Handlers
{
    using System;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class StreamRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly StreamRequest request;
        private readonly InfoBuilder infoBuilder;

        public StreamRequestHandler(IContainerManager containerManager, Request request)
            : base(request)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            this.infoBuilder = new InfoBuilder(containerManager);
            this.request = (StreamRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            log.Trace("Handle: '{0}' JobId: '{1}''", request.Handle, request.JobId);
            return new StreamResponse
            {
                Data = "DATA TODO\n\n",
                ExitStatus = 0,
                Name = "stdout",
                Info = infoBuilder.GetInfoResponseFor(request.Handle)
            };
        }
    }
}
