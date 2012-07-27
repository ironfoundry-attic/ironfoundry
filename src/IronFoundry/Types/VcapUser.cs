using System;
using Newtonsoft.Json;

namespace IronFoundry.Types
{
    [Serializable]
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