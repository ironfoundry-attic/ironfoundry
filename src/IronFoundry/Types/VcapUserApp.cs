using Newtonsoft.Json;

namespace IronFoundry.Types
{
    public class VcapUserApp
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }
    }
}