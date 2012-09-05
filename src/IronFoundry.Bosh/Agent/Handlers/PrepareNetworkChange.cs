namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class PrepareNetworkChange : BaseMessageHandler
    {
        public PrepareNetworkChange(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            return new HandlerResponse(true);
        }
    }
}