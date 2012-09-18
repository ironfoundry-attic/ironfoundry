using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    using System;
    using Newtonsoft.Json;

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