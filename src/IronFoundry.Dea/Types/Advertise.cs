namespace IronFoundry.Dea.Types
{
    using System;
    using IronFoundry.Dea.JsonConverters;
    using Newtonsoft.Json;

    public class Advertise : Message
    {
        private static readonly string[] runtimes = new[] { Constants.SupportedRuntime };

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

        [JsonProperty(PropertyName = "currently_pending")]
        public ushort CurrentlyPending { get; private set; }

        [JsonProperty(PropertyName = "ready")]
        public bool Ready { get; private set; }

        [JsonProperty(PropertyName = "runtimes")]
        public string[] Runtimes
        {
            get { return runtimes; }
        }

        public Advertise(Guid id, uint availableMemory, ushort currentlyPending, bool ready)
        {
            this.ID               = id;
            this.AvailableMemory  = availableMemory;
            this.CurrentlyPending = currentlyPending;
            this.Ready            = ready;
        }
    }
}