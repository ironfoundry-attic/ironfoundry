namespace IronFoundry.Warden.Handlers
{
    using System;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Jobs;
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class StreamRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IJobManager jobManager;
        private readonly StreamRequest request;
        private readonly InfoBuilder infoBuilder;

        public StreamRequestHandler(IContainerManager containerManager, IJobManager jobManager, Request request)
            : base(request)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            if (jobManager == null)
            {
                throw new ArgumentNullException("jobManager");
            }
            this.jobManager = jobManager;
            this.infoBuilder = new InfoBuilder(containerManager);
            this.request = (StreamRequest)request;
        }

        public override Response Handle()
        {
            log.Trace("Handle: '{0}' JobId: '{1}''", request.Handle, request.JobId);
            var job = jobManager.GetJob(request.JobId);
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
