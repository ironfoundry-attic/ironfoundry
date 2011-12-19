using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using CloudFoundry.Net;

namespace CloudFoundry.Net.Types
{
    public class ExternalInstance : EntityBase
    {
        [JsonProperty(PropertyName = "instances")]
        public InstanceDetail[] ExternInstance { get; set; }
    }

    public class InstanceDetail : EntityBase
    {
        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "since")]
        public int Since { get; set; }

        [JsonProperty(PropertyName = "debug_port")]
        public string DebugPort { get; set; }
    }
}
