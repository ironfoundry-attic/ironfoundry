using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    using Newtonsoft.Json;

    public class Limits : EntityBase
    {
        [JsonProperty(PropertyName = "mem")]
        public int Mem { get; set; }

        [JsonProperty(PropertyName = "disk")]
        public int Disk { get; set; }

        [JsonProperty(PropertyName = "fds")]
        public int FDs { get; set; }
    }
}