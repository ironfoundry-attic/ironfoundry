namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json;
    
    public class StateResult
    {
        private const string NtpFileMissing = "file missing";
        private readonly string agentID;
        private readonly object vm;
        private readonly string serviceGroupState;
        private readonly string boshProtocol;

        public StateResult(string agentID, object vm, string serviceGroupState, string boshProtocol)
        {
            this.agentID = agentID;
            this.vm = vm;
            this.serviceGroupState = serviceGroupState; // agent/lib/agent/monit.rb running, starting, failing or unknown
            this.boshProtocol = boshProtocol;
        }

        [JsonProperty(PropertyName = "agent_id")]
        public string AgentID { get { return agentID; } }

        [JsonProperty(PropertyName = "vm")]
        public object VM { get { return vm; } }

        [JsonProperty(PropertyName = "job_state")]
        public string ServiceGroupState { get { return serviceGroupState; } }

        [JsonProperty(PropertyName = "bosh_protocol")]
        public string BoshProtocol { get { return boshProtocol; } }

        [JsonProperty(PropertyName = "ntp")]
        public string Ntp { get { return NtpFileMissing; } }
    }
}