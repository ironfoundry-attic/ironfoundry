namespace IronFoundry.Types
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [Serializable]
    public class ProvisionedService : EntityBase, IMergeable<ProvisionedService>
    {        
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; private set; }

        [JsonProperty(PropertyName = "tier")]
        public string Tier { get; private set; }

        [JsonProperty(PropertyName = "vendor")]
        public string Vendor { get; private set; }

        [JsonProperty(PropertyName = "meta")]
        public Meta MetaData { get; private set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; private set; }

        public void Merge(ProvisionedService obj)
        {            
            this.Type = obj.Type;
            this.Tier = obj.Tier;
            this.Vendor = obj.Vendor;
            this.MetaData = obj.MetaData;
            this.Version = obj.Version;
        }
    }

    [Serializable]
    public class Meta : EntityBase
    {
        [JsonProperty(PropertyName = "created")]
        public uint Created { get; set; }

        [JsonProperty(PropertyName = "updated")]
        public uint Updated { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public string[] Tags { get; set; }
        
        [JsonProperty(PropertyName = "version")]
        public uint Version { get; set; }
    }

    public class ProvisionedServiceEqualityComparer : IEqualityComparer<ProvisionedService>
    {
        public bool Equals(ProvisionedService c1, ProvisionedService c2)
        {
            return c1.Name.Equals(c2.Name);
        }

        public int GetHashCode(ProvisionedService c)
        {
            return c.Name.GetHashCode();
        }
    }
}
