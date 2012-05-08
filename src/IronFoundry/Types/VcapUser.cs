using Newtonsoft.Json;

namespace IronFoundry.Types
{
    public class VcapUser
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "admin")]
        public bool Admin { get; set; }

        [JsonProperty(PropertyName = "apps")]
        public VcapUserApp[] Apps { get; set; }
    }
}