namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json.Linq;

    public class Start : BaseMessageHandler
    {
        public override HandlerResponse Handle(JObject parsed)
        {
            // TODO: Monit.start_services
            // agent/lib/agent/handler.rb
            return new HandlerResponse("started");
        }
    }
}