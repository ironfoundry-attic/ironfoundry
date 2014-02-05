using System;
using IronFoundry.Dea.JsonConverters;
using Newtonsoft.Json;

namespace IronFoundry.Dea.Types
{
    public class InstanceExited : Message
    {
        private const string publishSubject = "droplet.exited";

        public InstanceExited(Instance instance)
        {
            ID = instance.DropletID;
            Version = instance.Version;
            InstanceID = instance.InstanceID;
            InstanceIndex = instance.InstanceIndex;
            Reason = instance.ExitReason;

            if (instance.IsCrashed)
            {
                CrashTimestamp = instance.StateTimestamp;
            }
        }

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return publishSubject; }
        }

        [JsonProperty(PropertyName = "droplet")]
        public Guid ID { get; private set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; private set; }

        [JsonProperty(PropertyName = "instance_id"), JsonConverter(typeof (VcapGuidConverter))]
        public Guid InstanceID { get; private set; }

        [JsonProperty(PropertyName = "index")]
        public uint InstanceIndex { get; private set; }

        [JsonProperty(PropertyName = "reason")]
        public string Reason { get; private set; }

        [JsonProperty(PropertyName = "crash_timestamp")]
        public int CrashTimestamp { get; private set; }
    }
}