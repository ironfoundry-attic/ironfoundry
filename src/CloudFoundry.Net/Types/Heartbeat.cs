namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public class Heartbeat : Message
    {
        public Heartbeat(Instance argInstance)
        {
            Droplet        = argInstance.DropletID;
            Version        = argInstance.Version;
            Instance       = argInstance.InstanceID;
            Index          = argInstance.InstanceIndex;
            State          = argInstance.State;
            StateTimestamp = argInstance.StateTimestamp;
        }

        [JsonProperty(PropertyName = "droplet")]
        public uint Droplet { get; set; } // TODO private setters?

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "instance")]
        public string Instance { get; set; }

        [JsonProperty(PropertyName = "index")]
        public string Index { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "state_timestamp")]
        public int StateTimestamp { get; set; }
    }
}