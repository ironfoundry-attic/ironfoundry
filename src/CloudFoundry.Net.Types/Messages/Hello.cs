namespace CloudFoundry.Net.Types.Messages
{
    using System.Net;
    using CloudFoundry.Net.Types.JsonConverters;
    using Newtonsoft.Json;
    
    public class Hello : Message
    {
        [JsonProperty(PropertyName="id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName="ip"), JsonConverter(typeof(IPAddressConverter))]
        public IPAddress IPAddress { get; set; }

        [JsonProperty(PropertyName="port")]
        public ushort Port { get; set; }

        [JsonProperty(PropertyName="version")]
        public decimal Version { get; set; }
    }
}