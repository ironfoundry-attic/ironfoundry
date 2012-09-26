using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    using Newtonsoft.Json;

    public class InfoUsage : EntityBase
    {
        [JsonProperty(PropertyName = "memory")]
        public uint Memory { get; private set; }

        [JsonProperty(PropertyName = "apps")]
        public uint Apps { get; private set; }

        [JsonProperty(PropertyName = "services")]
        public uint Services { get; private set; }
    }
}