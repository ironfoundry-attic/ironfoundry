namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class Shutdown : BaseMessageHandler
    {
        public Shutdown(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            return new HandlerResponse("shutdown");
        }

        public override void OnPostReply()
        {
            // TODO do shutdown steps here
        }
    }
}