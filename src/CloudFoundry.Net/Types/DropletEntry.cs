namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public class DropletEntry : JsonBase
    {
        [JsonProperty(PropertyName = "droplet")]
        public uint DropletID { get; set; }

        [JsonProperty(PropertyName = "instances")]
        public InstanceEntry[] Instances;
    }
}