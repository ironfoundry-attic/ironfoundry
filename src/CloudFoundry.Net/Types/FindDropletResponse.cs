namespace CloudFoundry.Net.Types
{
    using System;
    using JsonConverters;
    using Newtonsoft.Json;

    public class FindDropletResponse : Message
    {
        [JsonIgnore]
        public override string PublishSubject
        {
            get { return REPLY_OK; } // Find Droplet has no message specific subject.
        }

        [JsonProperty(PropertyName = "dea"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid Dea { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "droplet")]
        public uint Droplet { get; set; }

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
        public string Credentials { get; set; }

        [JsonProperty(PropertyName = "staged")]
        public string Staged { get; set; }

        [JsonProperty(PropertyName = "stats")]
        public Stats Stats { get; set; }
    }
}