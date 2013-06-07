namespace IronFoundry.Warden.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
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

        public void ObserveStatus(IJobStatus jobStatus)
        {
            if (messageWriter == null)
            {
                throw new InvalidOperationException("messageWriter can not be null when job status observed.");
            }
            StreamResponse response = ToStreamResponse(jobStatus);
            messageWriter.Write(response);
        }

        public StreamResponse Handle(MessageWriter messageWriter, CancellationToken cancellationToken)
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

            Job job = GetJobById(ref streamResponse);
            if (job != null)
            {
                job.AttachListener(this);
                job.Listen();
                streamResponse = new StreamResponse { ExitStatus = 0, Data = "TODO FINAL RESPONSE", Name = "stdout" };
            }

            return streamResponse;
        }

        public override Response Handle()
        {
            log.Trace("Handle: '{0}' JobId: '{1}''", request.Handle, request.JobId);

            StreamResponse streamResponse = null;
            Job job = GetJobById(ref streamResponse);
            if (job != null)
            {
                IJobStatus status = job.Status;
                if (status == null)
                {
                    log.Warn("Could not get status for job with ID '{0}'", request.JobId);
                }
                else
                {
                    streamResponse = ToStreamResponse(status);
                }
            }

            if (streamResponse != null)
            {
                InfoResponse info = infoBuilder.GetInfoResponseFor(request.Handle);
                streamResponse.Info = info;
            }
            return streamResponse;
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
            unchecked
            {
                return new StreamResponse
                {
                    ExitStatus = (uint)result.ExitCode
                };
            }
        }

        private static StreamResponse ToStreamResponse(IJobStatus status)
        {
            var streamResponse = new StreamResponse
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
            }
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
