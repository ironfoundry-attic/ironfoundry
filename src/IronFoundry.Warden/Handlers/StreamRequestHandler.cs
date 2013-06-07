namespace IronFoundry.Warden.Handlers
{
    using System;
    using System.Collections.Generic;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Jobs;
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class StreamRequestHandler : RequestHandler, IStreamingHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IJobManager jobManager;
        private readonly StreamRequest request;
        private readonly InfoBuilder infoBuilder;

        private bool complete = false;

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

        public bool Complete
        {
            get { return complete; }
        }

        public override Response Handle()
        {
            log.Trace("Handle: '{0}' JobId: '{1}''", request.Handle, request.JobId);

            StreamResponse streamResponse = null;

            var job = jobManager.GetJob(request.JobId);
            if (job == null)
            {
                streamResponse = GetErrorResponse(true, "Error! Expected to find job with ID '{0}' but could not.", request.JobId);
                complete = true;
            }
            else
            {
                IJobStatus status = job.Status;
                if (status == null)
                {
                    // TODO: should this even get sent back?
                    streamResponse = GetErrorResponse(false, "Warning: could not get status for job with ID '{0}'", request.JobId);
                }
                else
                {
                    streamResponse = new StreamResponse
                    {
                        Data = status.Data,
                        Name = status.DataSource.ToString()
                    };

                    if (status.ExitStatus.HasValue)
                    {
                        unchecked
                        {
                            streamResponse.ExitStatus = (uint)status.ExitStatus.Value;
                        }
                        complete = true;
                    }
                }
            }

            InfoResponse info = infoBuilder.GetInfoResponseFor(request.Handle);
            streamResponse.Info = info;
            return streamResponse;
        }

        private static StreamResponse GetErrorResponse(bool errorExitStatus, string fmt, params object[] args)
        {
            if (fmt.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("fmt");
            }

            if (args.IsNullOrEmpty())
            {
                throw new ArgumentNullException("args");
            }

            var response = new StreamResponse
            {
                Data = String.Format(fmt, args),
                Name = JobDataSource.stderr.ToString(),
            };

            if (errorExitStatus)
            {
                response.ExitStatus = 1;
            }

            return response;
        }
    }
}
