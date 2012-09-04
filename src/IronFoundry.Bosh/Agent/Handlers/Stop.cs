namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json.Linq;

    public class Stop : BaseMessageHandler
    {
        public override HandlerResponse Handle(JObject parsed)
        {
            // TODO long_running? is true
            // agent/lib/agent/handler.rb 410
            // Monit.stop_services
            return new HandlerResponse("stopped");
        }
    }
}