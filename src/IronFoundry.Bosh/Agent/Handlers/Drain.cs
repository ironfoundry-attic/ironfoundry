namespace IronFoundry.Bosh.Agent.Handlers
{
    using System;
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class Drain : BaseMessageHandler
    {
        public Drain(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            throw new NotImplementedException();
        }
    }
}