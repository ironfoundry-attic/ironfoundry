using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    using Newtonsoft.Json;

    public class VcapUserApp
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }
    }
}