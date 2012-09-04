namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json.Linq;

    public class Ping : BaseMessageHandler
    {
        public override HandlerResponse Handle(JObject parsed)
        {
            return new HandlerResponse("pong");
        }
    }
}