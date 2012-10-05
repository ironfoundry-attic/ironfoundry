namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class State : BaseMessageHandler
    {
        public State(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            // agent/lib/agent/monit.rb running, starting, failing or unknown
            var stateResult = new StateResult(config.AgentID, config.VM, "running", config.BoshProtocol);
            return new HandlerResponse(stateResult);
        }
    }
}