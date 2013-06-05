namespace IronFoundry.Warden.Handlers
{
    using System;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Run;
    using IronFoundry.Warden.Protocol;

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

        protected ScriptRunner GetScriptRunnerFor(string handle, string script)
        {
            Container c = containerManager.GetContainer(handle);
            return new ScriptRunner(c, script);
        }
    }
}
