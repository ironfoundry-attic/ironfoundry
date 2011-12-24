namespace IronFoundry.Dea.Types
{
    using System;
    using JsonConverters;
    using Newtonsoft.Json;

    /// <summary>
    /// router.register
    /// </summary>
    public class RouterRegister : Message
    {
        private const string publishSubject = "router.register";

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return publishSubject; }
        }

        [JsonProperty(PropertyName = "dea"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid Dea { get; set; }

        [JsonProperty(PropertyName = "host")]
        public string Host { get; set; }

        [JsonProperty(PropertyName = "port")]
        public ushort Port { get; set; }

        [JsonProperty(PropertyName = "uris")]
        public string[] Uris { get; set; }

        [JsonProperty(PropertyName = "tags")] // TODO why tags plural?
        public Tag Tag { get; set; }
    }
}