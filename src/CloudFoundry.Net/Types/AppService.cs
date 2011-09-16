using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Types
{
    public class AppService : JsonBase
    {
        
        [JsonProperty(PropertyName = "name")]
        string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        string Type { get; set; }

        [JsonProperty(PropertyName = "vendor")]
        string Vendor { get; set; }

        [JsonProperty(PropertyName = "meta")]
        Meta MetaData { get; set; }

        [JsonProperty(PropertyName = "properties")]
        Properties Props { get; set; }
        public AppService () {
            MetaData = new Meta();
            Props = new Properties();
        }
    }

    class Meta : JsonBase
    {
        [JsonProperty(PropertyName = "created")]
        int Created { get; set; }

        [JsonProperty(PropertyName = "updated")]
        int Updated { get; set; }

        [JsonProperty(PropertyName = "tags")]
        string[] Tags { get; set; }
        
        [JsonProperty(PropertyName = "version")]
        int Version { get; set; }
    }

    class Properties : JsonBase {

        //have to discover what this is off the ruby code
    }
}
