namespace IronFoundry.Bosh.Agent
{
    using Newtonsoft.Json;

    public class VMSetupState
    {
        [JsonProperty(PropertyName = "is_sysprepped")]
        public bool IsSysprepped { get; set; }

        [JsonProperty(PropertyName = "is_network_setup")]
        public bool IsNetworkSetup { get; set; }
    }
}