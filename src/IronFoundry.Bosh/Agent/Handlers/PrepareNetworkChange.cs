namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json.Linq;

    public class PrepareNetworkChange : BaseMessageHandler
    {
        public override HandlerResponse Handle(JObject parsed)
        {
            return new HandlerResponse(true);
        }
    }
}