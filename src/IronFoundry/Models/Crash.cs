using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    using Newtonsoft.Json;

    public class Crash : EntityBase
    {
        [JsonProperty(PropertyName = "instance")]
        public string Instance { get; private set; }

        [JsonProperty(PropertyName = "since")]
        public int Since { get; private set; }
    }
}
