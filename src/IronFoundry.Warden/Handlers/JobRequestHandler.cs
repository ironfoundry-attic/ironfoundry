namespace IronFoundry.Warden.Handlers
{
    using System;
    using IronFoundry.Warden.Jobs;
    using IronFoundry.Warden.Protocol;

    public abstract class JobRequestHandler : RequestHandler
    {
        protected readonly IJobManager jobManager;

        public JobRequestHandler(IJobManager jobManager, Request request)
            : base(request)
        {
            if (jobManager == null)
            {
                throw new ArgumentNullException("jobManager");
            }
            this.jobManager = jobManager;
        }
    }
}
