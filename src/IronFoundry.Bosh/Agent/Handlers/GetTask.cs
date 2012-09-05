namespace IronFoundry.Bosh.Agent.Handlers
{
    using System;
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class GetTask : BaseMessageHandler
    {
        public GetTask(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            throw new NotImplementedException();
        }
    }
}