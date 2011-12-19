namespace IronFoundry.Dea.Types
{
    using System;
    using JsonConverters;
    using Newtonsoft.Json;

    /// <summary>
    /// Used for router.unregister
    /// </summary>
    public class RouterUnregister : Message
    {
        private const string publishSubject = "router.unregister";

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
        public string[] Uris { get; set; } // TODO actual System.Uri
    }
}