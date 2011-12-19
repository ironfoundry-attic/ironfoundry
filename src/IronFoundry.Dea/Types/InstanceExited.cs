namespace IronFoundry.Dea.Types
{
    using System;
    using JsonConverters;
    using Newtonsoft.Json;

    public class InstanceExited : Message
    {
        private const string publishSubject = "droplet.exited";

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return publishSubject; }
        }

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

        public InstanceExited(Instance argInstance)
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