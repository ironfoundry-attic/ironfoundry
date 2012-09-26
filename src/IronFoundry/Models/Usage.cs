using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    using System;
    using Newtonsoft.Json;

    public class Usage
     {
        [JsonProperty(PropertyName="time")]
        public DateTime CurrentTime { get; set; }

        [JsonProperty(PropertyName="cpu")]
        public float CpuTime { get; set; }

        [JsonProperty(PropertyName="mem")]
        public float MemoryUsage { get; set; }

        [JsonProperty(PropertyName="disk")]
        public float DiskUsage { get; set; }
    }
}