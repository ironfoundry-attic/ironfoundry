namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class Start : BaseMessageHandler
    {
        public Start(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            // TODO: Monit.start_services
            // agent/lib/agent/handler.rb
            return new HandlerResponse("started");
        }
    }
}