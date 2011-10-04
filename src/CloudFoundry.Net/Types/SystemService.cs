using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Types
{
    [Serializable]
    public class SystemServices : EntityBase
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } //Types supported are key/value, generic, database... could potentially be a static class or enum

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "vendor")]
        public string Vendor { get; set; }

        [JsonProperty(PropertyName = "tiers")]
        public Tiers Tiers { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        public SystemServices()
        {
            Tiers = new Tiers();
        }
    }

    [Serializable]
    public class Tiers : EntityBase
    {
        [JsonProperty(PropertyName = "free")]
        public Type Type { get; set; } //Currently on showing Free but potentially other options in the future

        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }

        public Tiers()
        {
            Type = new Type();
        }
    }

    [Serializable]
    public class Type : EntityBase
    {
        [JsonProperty(PropertyName = "options")]
        Options Options { get; set; }

        public Type()
        {
            Options = new Options();
        }
    }
    
    [Serializable]
    public class Options : EntityBase
    {

    }



}
