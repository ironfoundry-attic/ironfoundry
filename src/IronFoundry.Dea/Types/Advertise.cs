namespace IronFoundry.Dea.Types
{
    using System;
    using IronFoundry.Dea.JsonConverters;
    using Newtonsoft.Json;

    public class Advertise : Message
    {
        private const string publishSubject = "dea.advertise";

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return publishSubject; }
        }

        [JsonProperty(PropertyName="id"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid ID { get; private set; }

        [JsonProperty(PropertyName = "available_memory")]
        public uint AvailableMemory { get; private set; }

        [JsonProperty(PropertyName = "runtimes")]
        public string[] Runtimes
        {
            get { return Constants.SupportedRuntimes; }
        }

        [JsonProperty(PropertyName = "prod")]
        public bool Prod { get; private set; }

        [JsonProperty(PropertyName = "ready")]
        public bool Ready { get { return true; } }

        [JsonProperty(PropertyName = "currently_pending")]
        public ushort CurrentlyPending { get { return 0; } }

        public Advertise(Guid id, uint availableMemory, bool prod)
        {
            this.ID              = id;
            this.AvailableMemory = availableMemory;
            this.Prod            = prod;
        }
    }
}