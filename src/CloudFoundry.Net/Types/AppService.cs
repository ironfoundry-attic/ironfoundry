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
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "vendor")]
        public string Vendor { get; set; }

        [JsonProperty(PropertyName = "meta")]
        public Meta MetaData { get; set; }

        [JsonProperty(PropertyName = "version")]
        public float Version { get; set; }

        [JsonProperty(PropertyName = "properties")]
        public Properties Props { get; set; }
        public AppService () {
            MetaData = new Meta();
            Props = new Properties();
        }
    }

    public class Meta : JsonBase
    {
        [JsonProperty(PropertyName = "created")]
        public int Created { get; set; }

        [JsonProperty(PropertyName = "updated")]
        public int Updated { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public string[] Tags { get; set; }
        
        [JsonProperty(PropertyName = "version")]
        public int Version { get; set; }
    }

    public class Properties : JsonBase {

        //have to discover what this is off the ruby code
    }
}
