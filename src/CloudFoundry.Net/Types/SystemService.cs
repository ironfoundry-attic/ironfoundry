namespace CloudFoundry.Net.Types
{
    using System;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [Serializable]
    public class SystemService : EntityBase
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } //Types supported are key/value, generic, database... could potentially be a static class or enum

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "vendor")]
        public string Vendor { get; set; }

        /*
         * TODO
        [JsonProperty(PropertyName = "tiers")]
        public Tiers Tiers { get; set; }
         */

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }

    public class SystemServiceEqualityComparer : IEqualityComparer<SystemService>
    {
        public bool Equals(SystemService c1, SystemService c2)
        {
            return c1.Vendor.Equals(c2.Vendor);
        }

        public int GetHashCode(SystemService c)
        {
            return c.Vendor.GetHashCode();
        }
    }

    /*
    [Serializable]
    public class Tiers : EntityBase
    {
        [JsonProperty(PropertyName = "free")]
        public Type Type { get; set; } //Currently on showing Free but potentially other options in the future

        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }
    }

    [Serializable]
    public class Type : EntityBase
    {
        [JsonProperty(PropertyName = "options")]
        public Options Options { get; set; }
    }
    
    [Serializable]
    public class Options : EntityBase { }
     */
}