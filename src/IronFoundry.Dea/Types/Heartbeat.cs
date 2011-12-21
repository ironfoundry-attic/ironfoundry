namespace IronFoundry.Dea.Types
{
    using System;
    using IronFoundry.Dea.JsonConverters;
    using Newtonsoft.Json;

    public class Heartbeat : Message
    {
        public Heartbeat(Instance argInstance)
        {
            Droplet        = argInstance.DropletID;
            Version        = argInstance.Version;
            InstanceID     = argInstance.InstanceID;
            Index          = argInstance.InstanceIndex;
            State          = argInstance.State;
            StateTimestamp = argInstance.StateTimestamp;
        }

        [JsonProperty(PropertyName = "droplet")]
        public uint Droplet { get; private set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; private set; }

        [JsonProperty(PropertyName = "instance"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid InstanceID { get; private set; }

        [JsonProperty(PropertyName = "index")]
        public uint Index { get; private set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; private set; }

        [JsonProperty(PropertyName = "state_timestamp")]
        public int StateTimestamp { get; private set; }
    }
}