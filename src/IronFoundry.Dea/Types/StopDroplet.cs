namespace IronFoundry.Dea.Types
{
    using System;
    using System.Collections;
    using System.Linq;
    using JsonConverters;
    using Newtonsoft.Json;

    public class StopDroplet : Message
    {
        [JsonProperty(PropertyName = "droplet")]
        public uint DropletID { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "instance_ids"), JsonConverter(typeof(VcapGuidArrayConverter))]
        public Guid[] InstanceIDs { get; set; }

        [JsonProperty(PropertyName = "indices")]
        public uint[] InstanceIndices { get; set; }

        [JsonProperty(PropertyName = "states")]
        public string[] InstanceStates { get; set; }

        public bool AppliesTo(Instance argInstance)
        {
            bool versionMatched = String.IsNullOrWhiteSpace(Version) || Version == argInstance.Version;
            bool instanceMatched = InstanceIDs.IsNullOrEmpty() || InstanceIDs.Contains(argInstance.InstanceID);
            bool indexMatched = InstanceIndices.IsNullOrEmpty() || InstanceIndices.Contains(argInstance.InstanceIndex);
            bool stateMatched = InstanceStates.IsNullOrEmpty() || InstanceStates.Contains(argInstance.State);

            return versionMatched && instanceMatched && indexMatched && stateMatched;
        }
    }
}