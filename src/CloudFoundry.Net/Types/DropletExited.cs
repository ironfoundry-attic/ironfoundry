namespace CloudFoundry.Net.Types
{
    using System;
    using Converters;
    using Newtonsoft.Json;

    /*
        :droplet => instance[:droplet_id],
        :version => instance[:version],
        :instance => instance[:instance_id],
        :index => instance[:instance_index],
        :reason => instance[:exit_reason],
        exit_message[:crash_timestamp] = instance[:state_timestamp] if instance[:state] == :CRASHED
     */
    public class DropletExited : Message
    {
        [JsonProperty(PropertyName = "droplet")]
        public uint ID { get; private set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; private set; }

        [JsonProperty(PropertyName = "instance_id"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid InstanceID { get; private set; }

        [JsonProperty(PropertyName = "index")]
        public uint InstanceIndex { get; private set; }

        [JsonProperty(PropertyName = "reason")]
        public string Reason { get; private set; }

        [JsonProperty(PropertyName = "crash_timestamp")]
        public int CrashTimestamp { get; private set; }

        public DropletExited(Instance argInstance)
        {
            ID            = argInstance.DropletID;
            Version       = argInstance.Version;
            InstanceID    = argInstance.InstanceID;
            InstanceIndex = argInstance.InstanceIndex;
            Reason        = String.Empty; // TODO

            if (argInstance.IsCrashed)
            {
                CrashTimestamp = argInstance.StateTimestamp;
            }
        }
    }
}