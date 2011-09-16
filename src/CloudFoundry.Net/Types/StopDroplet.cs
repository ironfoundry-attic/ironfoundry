namespace CloudFoundry.Net.Types
{
    using System;
    using JsonConverters;
    using Newtonsoft.Json;

    public class StopDroplet : Message
    {
        [JsonProperty(PropertyName = "droplet")]
        public uint ID { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "instance_ids"), JsonConverter(typeof(VcapGuidArrayConverter))]
        public Guid[] InstanceIDs { get; set; }

        [JsonProperty(PropertyName = "indices")]
        public uint[] InstanceIndices { get; set; }

        [JsonProperty(PropertyName = "states")]
        public string[] InstanceStates { get; set; }
    }
}