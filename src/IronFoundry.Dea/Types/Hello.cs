namespace IronFoundry.Dea.Types
{
    using System;
    using System.Net;
    using IronFoundry.Dea.JsonConverters;
    using Newtonsoft.Json;
    
    public class Hello : Message
    {
        private const string publishSubject = "dea.start";

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return publishSubject; }
        }

        [JsonProperty(PropertyName="id"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid ID { get; private set; }

        [JsonProperty(PropertyName="ip"), JsonConverter(typeof(IPAddressConverter))]
        public IPAddress IPAddress { get; private set; }

        [JsonProperty(PropertyName="port")]
        public ushort Port { get; private set; }

        [JsonProperty(PropertyName="version")]
        public decimal Version { get; private set; }

        public Hello(Guid argID, IPAddress argAddress, ushort filesServicePort)
        {
            ID        = argID;
            IPAddress = argAddress;
            Port      = filesServicePort;
            Version   = 0.99m;
        }

        public override bool CanPublishWithSubject(string subject)
        {
            return false == subject.IsNullOrWhiteSpace();
        }
    }
}