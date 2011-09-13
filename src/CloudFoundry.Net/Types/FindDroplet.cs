namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public class FindDroplet : Message
    {
        [JsonProperty(PropertyName = "droplet")]
        public uint DropletID { get; set; }

        [JsonProperty(PropertyName = "indices")]
        public int[] Indices { get; set; }

        [JsonProperty(PropertyName = "states")]
        public string[] States { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }
    }
}