namespace CloudFoundry.Net.Types.Entities
{
    using Newtonsoft.Json;

    public class Snapshot : JsonBase
    {
        [JsonProperty(PropertyName = "entries")]
        public DropletEntry[] Entries { get; set; }
    }
}