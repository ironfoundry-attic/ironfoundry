namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Jobs;
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class RunRequestHandler : TaskRequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly RunRequest request;
        private readonly InfoBuilder infoBuilder;

        public RunRequestHandler(IContainerManager containerManager, Request request)
            : base(containerManager, request)
        {
            this.infoBuilder = new InfoBuilder(containerManager);
            this.request = (RunRequest)request;
        }

        public override Response Handle()
        {
            log.Trace("Handle: '{0}' Script: '{1}'", request.Handle, request.Script);

            IJobRunnable runnable = base.GetRunnableFor(request);
            IJobResult result = runnable.Run(); // run synchronously

            unchecked
            {
                return new RunResponse
                {
                    ExitStatus = (uint)result.ExitCode,
                    Stdout = result.Stdout,
                    Stderr = result.Stderr,
                    Info = infoBuilder.GetInfoResponseFor(request.Handle)
                };
            }
        }
    }
}
