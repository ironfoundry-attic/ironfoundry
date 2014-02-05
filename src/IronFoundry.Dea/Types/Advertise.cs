using System;
using System.Collections.Generic;
using IronFoundry.Dea.JsonConverters;
using Newtonsoft.Json;

namespace IronFoundry.Dea.Types
{
    public class Advertise : Message
    {
        public Advertise(Guid id, uint availableMemory)
        {
            ID = id;
            AvailableMemory = availableMemory;
            AvailableDisk = 16*1024;
            AppIdToCount = new Dictionary<Guid, uint>();
        }

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return "dea.advertise"; }
        }

        [JsonProperty(PropertyName = "id"), JsonConverter(typeof (VcapGuidConverter))]
        public Guid ID { get; private set; }

        [JsonProperty(PropertyName = "available_memory")]
        public uint AvailableMemory { get; private set; }

        [JsonProperty(PropertyName = "available_disk")]
        public uint AvailableDisk { get; private set; }

        [JsonProperty(PropertyName = "app_id_to_count")]
        public Dictionary<Guid, uint> AppIdToCount { get; private set; }

        [JsonProperty(PropertyName = "stacks")]
        public string[] Stacks
        {
            get { return Constants.SupportedRuntimes; }
        }

        [JsonProperty(PropertyName = "placement_properties")]
        public Dictionary<string, string> PlacementProperties
        {
            get 
            { 
                return new Dictionary<string, string>()
                         {
                             {"zone", "default"}
                         }; 
            }
        }
    }
}