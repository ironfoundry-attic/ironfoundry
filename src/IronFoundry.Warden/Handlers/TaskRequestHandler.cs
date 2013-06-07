namespace IronFoundry.Warden.Handlers
{
    using System;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Jobs;
    using IronFoundry.Warden.Protocol;
    using IronFoundry.Warden.Tasks;

    public abstract class TaskRequestHandler : RequestHandler
    {
        protected readonly IContainerManager containerManager;

        public TaskRequestHandler(IContainerManager containerManager, Request request)
            : base(request)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            this.containerManager = containerManager;
        }

        protected IJobRunnable GetRunnableFor(ITaskRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Container container = containerManager.GetContainer(request.Handle);
            return new TaskRunner(container, request);
        }
    }
}
