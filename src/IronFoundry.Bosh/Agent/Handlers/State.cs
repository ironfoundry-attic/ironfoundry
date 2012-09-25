namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class State : BaseMessageHandler
    {
        public State(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            var stateResult = new StateResult(config.AgentID, config.VM, "starting", config.BoshProtocol);
            return new HandlerResponse(stateResult);
        }
    }
}