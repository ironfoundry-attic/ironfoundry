using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    using Newtonsoft.Json;

    public class Framework :EntityBase
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "runtimes")]
        public Runtime[] Runtimes { get; private set; }

        [JsonProperty(PropertyName="appservers")]
        public AppServer[] AppServers { get; private set; }
    }
}
