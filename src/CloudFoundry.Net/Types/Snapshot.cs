namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public class Snapshot : JsonBase
    {
        [JsonProperty(PropertyName = "entries")]
        public DropletEntry[] Entries { get; set; }
    }
}