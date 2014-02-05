using System;
using System.Collections;
using System.Linq;
using IronFoundry.Dea.JsonConverters;
using Newtonsoft.Json;

namespace IronFoundry.Dea.Types
{
    public class StopDroplet : Message
    {
        [JsonProperty(PropertyName = "droplet")]
        public Guid DropletID { get; set; }

        //[JsonProperty(PropertyName = "version")]
        //public string Version { get; set; }

        //[JsonProperty(PropertyName = "instances"), JsonConverter(typeof (VcapGuidArrayConverter))]
        //public Guid[] InstanceIDs { get; set; }

        //[JsonProperty(PropertyName = "indices")]
        //public uint[] InstanceIndices { get; set; }

        //[JsonProperty(PropertyName = "states")]
        //public string[] InstanceStates { get; set; }

        public bool AppliesTo(Instance instance)
        {
            return DropletID == instance.DropletID;
            //bool dropletMatched = DropletID == instance.DropletID;
            //bool versionMatched = Version.IsNullOrWhiteSpace() || Version == instance.Version;
            //bool instanceMatched = InstanceIDs.IsNullOrEmpty() || InstanceIDs.Contains(instance.InstanceID);
            //bool indexMatched = InstanceIndices.IsNullOrEmpty() || InstanceIndices.Contains(instance.InstanceIndex);
            //bool stateMatched = InstanceStates.IsNullOrEmpty() || InstanceStates.Contains(instance.State);

            //return dropletMatched && versionMatched && instanceMatched && indexMatched && stateMatched;
        }
    }
}