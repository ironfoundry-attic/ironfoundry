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

            string responseData = String.Empty;
            string responseName = JobDataSource.stdout.ToString();
            uint? exitStatus = null;
            InfoResponse info = infoBuilder.GetInfoResponseFor(request.Handle);

            var job = jobManager.GetJob(request.JobId);

            if (job == null)
            {
                responseData = String.Format("Error! Expected to find job with ID '{0}' but could not.", request.JobId);
                responseName = JobDataSource.stderr.ToString();
                exitStatus = 1;
            }
            else
            {
                IJobStatus status = job.Status;
                if (status != null)
                {
                    responseData = status.Data;
                    responseName = status.DataSource.ToString();
                    if (status.ExitStatus.HasValue)
                    {
                        unchecked
                        {
                            exitStatus = (uint)status.ExitStatus.Value;
                        }
                    }
                }
            }

            var response = new StreamResponse
            {
                Data = responseData,
                Name = responseName,
                Info = info
            };

            if (exitStatus.HasValue)
            {
                response.ExitStatus = exitStatus.Value;
            }

            return response;
        }
    }
}
