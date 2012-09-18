using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Info : Message
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "build")]
        public string Build { get; private set; }

        [JsonProperty(PropertyName = "support")]
        public string Support { get; private set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; private set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; private set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; private set; }

        [JsonProperty(PropertyName = "limits")]
        public InfoLimits Limits { get; private set; }

        [JsonProperty(PropertyName = "useage")]
        public InfoUsage Usage { get; private set; }

        [JsonProperty(PropertyName = "frameworks")]
        public Dictionary<string, Framework> Frameworks { get; private set; }
    }
}
