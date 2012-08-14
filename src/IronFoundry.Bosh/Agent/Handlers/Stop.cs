namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json.Linq;

    public class Stop : BaseMessageHandler
    {
        public override HandlerResponse Handle(JObject parsed)
        {
            return new HandlerResponse("stopped");
        }
    }
}