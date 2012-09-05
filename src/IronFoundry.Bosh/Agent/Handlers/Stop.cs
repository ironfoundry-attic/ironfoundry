namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class Stop : BaseMessageHandler
    {
        public Stop(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            // TODO long_running? is true
            // agent/lib/agent/handler.rb 410
            // Monit.stop_services
            return new HandlerResponse("stopped");
        }
    }
}