namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json.Linq;

    public class Shutdown : BaseMessageHandler
    {
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