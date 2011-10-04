namespace CloudFoundry.Net.Types
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [Serializable]
    public class AppService : EntityBase
    {        
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; private set; }

        [JsonProperty(PropertyName = "tiers")]
        public Tier Tier {get; private set;}

        [JsonProperty(PropertyName = "vendor")]
        public string Vendor { get; private set; }

        [JsonProperty(PropertyName = "meta")]
        public Meta MetaData { get; private set; }

        [JsonProperty(PropertyName = "version")]
        public float Version { get; private set; }

        [JsonProperty(PropertyName = "properties")]
        public Properties Props { get; private set; }
    }

    [Serializable]
    public class Tier : EntityBase
    {
        [JsonProperty(PropertyName = "options")]
        public Options Option { get; set; }

        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }
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
    
    [Serializable]
    public class Properties : EntityBase {

        //have to discover what this is off the ruby code
    }

    public class AppServiceEqualityComparer : IEqualityComparer<AppService>
    {
        public bool Equals(AppService c1, AppService c2)
        {
            return c1.Name.Equals(c2.Name);
        }

        public int GetHashCode(AppService c)
        {
            return c.Name.GetHashCode();
        }
    }
}
