namespace CloudFoundry.Net.Types.Messages
{
    using Newtonsoft.Json;
    using Entities;

    public class RouterRegister : Message
    {
        [JsonProperty(PropertyName = "dea")]
        public string Dea { get; set; } // TODO guid?

        [JsonProperty(PropertyName = "host")]
        public string Host { get; set; }

        [JsonProperty(PropertyName = "port")]
        public ushort Port { get; set; }

        [JsonProperty(PropertyName = "uris")]
        public string[] Uris { get; set; } // TODO actual System.Uri

        [JsonProperty(PropertyName = "tags")] // TODO why tags plural?
        public Tag Tag { get; set; }
    }
}