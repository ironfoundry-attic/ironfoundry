namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class FetchLogs : BaseMessageHandler
    {
        public FetchLogs(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            return new HandlerResponse("TODO");
        }
    }
}