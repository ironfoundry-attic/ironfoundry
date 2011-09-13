namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public class DropletHeartbeat : Message
    {
        [JsonProperty(PropertyName = "droplets")]
        public Heartbeat[] Droplets { get; set; }
    }
}
