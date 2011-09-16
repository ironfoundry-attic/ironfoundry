using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Types
{
    public class Crash : JsonBase
    {
        [JsonProperty(PropertyName = "instance")]
        string Instance { get; set; }

        [JsonProperty(PropertyName = "since")]
        int Since { get; set; }

    }
}
