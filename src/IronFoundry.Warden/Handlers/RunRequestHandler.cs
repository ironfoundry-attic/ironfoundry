namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class RunRequestHandler : RequestHandler
    {
        private readonly RunRequest request;

        public RunRequestHandler(Request request)
            : base(request)
        {
            this.request = (RunRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            return new RunResponse();
        }
    }
}
