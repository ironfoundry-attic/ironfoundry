namespace CloudFoundry.Net.Types
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Info : Message
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "build")]
        public string Build { get; private set; }

        [JsonProperty(PropertyName = "support")]
        public string Support { get; private set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; private set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; private set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; private set; }

        [JsonProperty(PropertyName = "limits")]
        public InfoLimits Limits { get; private set; }

        [JsonProperty(PropertyName = "useage")]
        public InfoUsage Usage { get; private set; }

        [JsonProperty(PropertyName = "frameworks")]
        public Dictionary<string, Framework> Frameworks { get; private set; }
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
        public uint Memory { get; private set; }

        [JsonProperty(PropertyName = "apps")]
        public uint Apps { get; private set; }

        [JsonProperty(PropertyName = "services")]
        public uint Services { get; private set; }
    }
}