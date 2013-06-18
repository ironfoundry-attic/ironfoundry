namespace IronFoundry.Warden.Handlers
{
    using System.Threading.Tasks;
    using Containers;
    using Jobs;
    using NLog;
    using Properties;
    using Protocol;

    public class LinkRequestHandler : JobRequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly LinkRequest request;

        public LinkRequestHandler(IContainerManager containerManager, IJobManager jobManager, Request request)
            : base(containerManager, jobManager, request)
        {
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

            response.Info = BuildInfoResponse();

            return response;
        }
    }
}
