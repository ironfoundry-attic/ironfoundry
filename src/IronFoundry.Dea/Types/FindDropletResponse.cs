namespace IronFoundry.Dea.Types
{
    using System;
    using JsonConverters;
    using Newtonsoft.Json;

    public class FindDropletResponse : Message
    {
        public FindDropletResponse(Guid argID, Instance argInstance)
        {
            Dea            = argID;
            Version        = argInstance.Version;
            Droplet        = argInstance.DropletID;
            InstanceID     = argInstance.InstanceID;
            Index          = argInstance.InstanceIndex;
            State          = argInstance.State;
            StateTimestamp = argInstance.StateTimestamp;
            Staged         = argInstance.Staged;

            if (this.State != VcapStates.RUNNING)
            {
                this.Stats = null;
            }
        }

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return ReplyOk; } // Find Droplet has no message specific subject.
        }

        [JsonProperty(PropertyName = "dea"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid Dea { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "droplet"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid Droplet { get; set; }

        [JsonProperty(PropertyName = "instance"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid InstanceID { get; set; }

        [JsonProperty(PropertyName = "index")]
        public uint Index { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "state_timestamp")]
        public int StateTimestamp { get; set; }

        [JsonProperty(PropertyName = "file_uri")]
        public string FileUri { get; set; }

        [JsonProperty(PropertyName = "credentials")]
        public string[] Credentials { get; set; }

        [JsonProperty(PropertyName = "staged")]
        public string Staged { get; set; }

        [JsonProperty(PropertyName = "stats")]
        public Stats Stats { get; set; }

        public override bool CanPublishWithSubject(string subject)
        {
            return false == subject.IsNullOrWhiteSpace();
        }
    }
}