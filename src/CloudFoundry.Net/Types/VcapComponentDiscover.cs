namespace CloudFoundry.Net.Types
{
    using System;
    using CloudFoundry.Net.Converters;
    using Newtonsoft.Json;

    public class VcapComponentDiscover : Message
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string Uuid { get; set; }

        [JsonProperty(PropertyName = "host")]
        public string Host { get; set; }

        [JsonProperty(PropertyName = "credentials")]
        public string Credentials { get; set; }

        [JsonProperty(PropertyName = "start"), JsonConverter(typeof(VcapDateTimeConverter))] // TODO
        public DateTime Start { get; set; }
    }
}