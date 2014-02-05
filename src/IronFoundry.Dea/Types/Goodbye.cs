using System;
using System.Net;
using IronFoundry.Dea.JsonConverters;
using Newtonsoft.Json;

namespace IronFoundry.Dea.Types
{
    public class Goodbye : Message
    {
        public Goodbye(Guid argID, IPAddress argAddress)
        {
            ID = argID;
            IPAddress = argAddress;
            Version = 0.99m;
            AppIdToCount = 0;
        }

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return "dea.shutdown"; }
        }

        [JsonProperty(PropertyName = "id"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid ID { get; private set; }

        [JsonProperty(PropertyName = "ip"), JsonConverter(typeof(IPAddressConverter))]
        public IPAddress IPAddress { get; private set; }

        [JsonProperty(PropertyName = "version")]
        public decimal Version { get; private set; }

        [JsonProperty(PropertyName = "app_id_to_count")]
        public int AppIdToCount { get; private set; }

        public override bool CanPublishWithSubject(string subject)
        {
            return false == subject.IsNullOrWhiteSpace();
        }
    }
}