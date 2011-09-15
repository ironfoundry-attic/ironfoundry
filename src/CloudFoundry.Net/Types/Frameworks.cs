using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Types
{
    class Frameworks : JsonBase
    {
        [JsonProperty(PropertyName = "frameworks")]
        Framework framework { get; set; }

        public Frameworks () {
            framework = new Framework();
        }
    }
}
