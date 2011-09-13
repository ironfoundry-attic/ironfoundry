namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public class Heartbeat : Message
    {
        [JsonProperty(PropertyName = "droplet")]
        public uint Droplet { get; set; }

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