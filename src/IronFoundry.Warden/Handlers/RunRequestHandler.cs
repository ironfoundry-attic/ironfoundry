namespace IronFoundry.Warden.Handlers
{
    using System;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Protocol;
    using IronFoundry.Warden.Utilities;
    using NLog;

    public class RunRequestHandler : RequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly RunRequest request;
        private readonly InfoBuilder infoBuilder;

        public RunRequestHandler(IContainerManager containerManager, Request request)
            : base(request)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            this.infoBuilder = new InfoBuilder(containerManager);
            this.request = (RunRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            log.Trace("Handle: '{0}' Script: '{1}'", request.Handle, request.Script);
            var runner = new ScriptRunner();
            return new RunResponse
            {
                ExitStatus = 0,
                Stderr = "TODO STDERR",
                Stdout = "TODO STDOUT",
                Info = infoBuilder.GetInfoResponseFor(request.Handle)
            };
        }
    }
}
