namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Protocol;
    using IronFoundry.Warden.Run;
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

            ScriptRunner runner = base.GetScriptRunnerFor(request.Handle, request.Script);
            var result = runner.Run();

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
