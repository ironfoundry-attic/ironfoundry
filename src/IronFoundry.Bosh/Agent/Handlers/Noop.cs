namespace IronFoundry.Bosh.Agent.Handlers
{
    using System;
    using Newtonsoft.Json.Linq;

    public class Noop : BaseMessageHandler
    {
        public override HandlerResponse Handle(JObject parsed)
        {
            throw new NotImplementedException();
        }
    }
}