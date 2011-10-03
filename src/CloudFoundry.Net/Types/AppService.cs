using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Types
{
    [Serializable]
    public class AppService : EntityBase
    {        
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "tiers")]
        public Tier Tier {get; set;}

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
            Tier = new Tier();
        }
    }

    [Serializable]
    public class Tier : EntityBase
    {
        [JsonProperty(PropertyName = "options")]
        public Options Option { get; set; }

        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }

        public Tier() {
            Option = new Options();
        }
    }

    //[Serializable]
    //public class Options : EntityBase
    //{
    //    //json does not contain defs..need to look in ruby code
    //}

    [Serializable]
    public class Meta : EntityBase
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
    
    public class Properties : EntityBase {

        //have to discover what this is off the ruby code
    }
}
