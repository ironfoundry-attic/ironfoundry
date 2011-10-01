namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public class Snapshot : EntityBase
    {
        [JsonProperty(PropertyName = "entries")]
        public DropletEntry[] Entries { get; set; }
    }
}