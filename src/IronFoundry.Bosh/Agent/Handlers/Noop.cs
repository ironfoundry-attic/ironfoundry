namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class Noop : BaseMessageHandler
    {
        public Noop(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            return new HandlerResponse("nope");
        }
    }
}