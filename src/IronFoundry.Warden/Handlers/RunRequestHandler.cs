namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;
    using NLog;

    public class RunRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly RunRequest request;

        public RunRequestHandler(Request request)
            : base(request)
        {
            this.request = (RunRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            log.Trace("Handle: '{0}' Script: '{1}'", request.Handle, request.Script);
            return new RunResponse { ExitStatus = 0, Stderr = "TODO STDERR", Stdout = "TODO STDOUT" };
        }
    }
}
