namespace CloudFoundry.Net.Types
{
    using System;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [Serializable]
    public class SystemService : EntityBase, IMergeable<SystemService>
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } 

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "vendor")]
        public string Vendor { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        public void Merge(SystemService obj)
        {
            this.Type = obj.Type;
            this.Version = obj.Version;
            this.Id = obj.Id;
            this.Description = obj.Description;
        }
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
}