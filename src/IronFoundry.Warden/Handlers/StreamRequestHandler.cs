namespace IronFoundry.Warden.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Jobs;
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class StreamRequestHandler : RequestHandler, IStreamingHandler, IJobListener
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IJobManager jobManager;
        private readonly StreamRequest request;
        private readonly InfoBuilder infoBuilder;

        private MessageWriter messageWriter;

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

        public void ListenStatus(IJobStatus jobStatus)
        {
            if (messageWriter == null)
            {
                throw new InvalidOperationException("messageWriter can not be null when job status observed.");
            }

            if (jobStatus == null)
            {
                log.Warn("Unexpected null job status!");
            }
            else
            {
                StreamResponse response = ToStreamResponse(jobStatus);
                messageWriter.Write(response);
            }
        }

        public async Task<StreamResponse> HandleAsync(MessageWriter messageWriter, CancellationToken cancellationToken)
        {
            if (messageWriter == null)
            {
                throw new ArgumentNullException("messageWriter");
            }
            if (cancellationToken == null)
            {
                throw new ArgumentNullException("cancellationToken");
            }

            this.messageWriter = messageWriter;

            log.Trace("HandleAsync: '{0}' JobId: '{1}''", request.Handle, request.JobId);

            StreamResponse streamResponse = null;

            Job job = GetJobById(ref streamResponse); // if job is null, streamResponse is set to error
            if (job != null)
            {
                if (job.HasStatus)
                {
                    foreach (IJobStatus status in job.Status)
                    {
                        ListenStatus(status);
                    }
                }

                if (job.IsCompleted)
                {
                    if (job.Result == null)
                    {
                        streamResponse = GetErrorResponse(true, "Error! Job with ID '{0}' is completed but no result is available!", request.JobId);
                    }
                    else
                    {
                        streamResponse = ToStreamResponse(job.Result);
                    }
                }
                else
                {
                    job.AttachListener(this);
                    IJobResult result = await job.ListenAsync();
                    streamResponse = StreamResponse.Create(result.ExitCode);
                }
            }

            return streamResponse;
        }

        public override Response Handle()
        {
            throw new InvalidOperationException("StreamRequestHandler implements IStreamingHandler so HandleAsync() should be called!");
        }

        private Job GetJobById(ref StreamResponse streamResponse)
        {
            Job job = jobManager.GetJob(request.JobId);
            if (job == null)
            {
                streamResponse = GetErrorResponse(true, "Error! Expected to find job with ID '{0}' but could not.", request.JobId);
            }
            return job;
        }

        private static StreamResponse ToStreamResponse(IJobResult result)
        {
            return StreamResponse.Create(result.ExitCode, result.Stdout, result.Stderr);
        }

        private static StreamResponse ToStreamResponse(IJobStatus status)
        {
            return StreamResponse.Create(status.ExitStatus, status.DataSource.ToString(), status.Data);
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
