namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class Ping : BaseMessageHandler
    {
        public Ping(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            return new HandlerResponse("pong");
        }
    }
}