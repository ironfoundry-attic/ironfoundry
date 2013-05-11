namespace IronFoundry.Warden.Handlers
{
    using IronFoundry.Warden.Protocol;

    public class SpawnRequestHandler : RequestHandler
    {
        private readonly SpawnRequest request;

        public SpawnRequestHandler(Request request)
            : base(request)
        {
            this.request = (SpawnRequest)request;
        }

        public override Response Handle()
        {
            // TODO do work!
            return new SpawnResponse();
        }
    }
}
