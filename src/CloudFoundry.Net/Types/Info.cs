namespace CloudFoundry.Net.Types
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Info : Message
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "build")]
        public string Build { get; set; }

        [JsonProperty(PropertyName = "support")]
        public string Support { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        [JsonProperty(PropertyName = "limits")]
        public InfoLimits Limits { get; set; }

        [JsonProperty(PropertyName = "useage")]
        public InfoUsage Usage { get; set; }

        [JsonProperty(PropertyName = "framework")]
        public IDictionary<string, Framework> Frameworks { get; set; }
    }

    public class InfoLimits : EntityBase
    {
        [JsonProperty(PropertyName = "memory")]
        public uint Memory { get; private set; }

        [JsonProperty(PropertyName = "app_uris")]
        public uint AppURIs { get; private set; }

        [JsonProperty(PropertyName = "services")]
        public uint Services { get; private set; }

        [JsonProperty(PropertyName = "apps")]
        public uint Apps { get; private set; }
    }

    public class InfoUsage : EntityBase
    {
        [JsonProperty(PropertyName = "memory")]
        public uint Memory { get; set; }

        [JsonProperty(PropertyName = "apps")]
        public uint Apps { get; set; }

        [JsonProperty(PropertyName = "services")]
        public uint Services { get; set; }
    }
}