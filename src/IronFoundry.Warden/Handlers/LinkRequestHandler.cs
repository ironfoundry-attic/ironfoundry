namespace IronFoundry.Warden.Handlers
{
    using System;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Jobs;
    using IronFoundry.Warden.Properties;
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class LinkRequestHandler : JobRequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly LinkRequest request;
        private readonly InfoBuilder infoBuilder;

        public LinkRequestHandler(IContainerManager containerManager, IJobManager jobManager, Request request)
            : base(jobManager, request)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            this.infoBuilder = new InfoBuilder(containerManager);
            this.request = (LinkRequest)request;
        }

        public async override Task<Response> HandleAsync()
        {
            log.Trace("Handle: '{0}' JobId: '{1}'", request.Handle, request.JobId);

            LinkResponse response = null;

            Job job = jobManager.GetJob(request.JobId);
            if (job == null)
            {
                ResponseData responseData = GetResponseData(true, Resources.JobRequestHandler_NoSuchJob_Message);
                response = new LinkResponse
                {
                    ExitStatus = (uint)responseData.ExitStatus,
                    Stderr = responseData.Message,
                };
            }
            else
            {
                IJobResult result = await job.RunnableTask;
                response = new LinkResponse
                {
                    ExitStatus = (uint)result.ExitCode,
                    Stderr = result.Stderr,
                    Stdout = result.Stdout,
                };
            }

            response.Info = infoBuilder.GetInfoResponseFor(request.Handle);

            return response;
        }
    }
}
