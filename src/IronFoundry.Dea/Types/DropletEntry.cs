using System;
using IronFoundry.Dea.JsonConverters;
using Newtonsoft.Json;

namespace IronFoundry.Dea.Types
{
    public class DropletEntry : EntityBase
    {
        [JsonProperty(PropertyName = "instances")] public InstanceEntry[] Instances;

        [JsonProperty(PropertyName = "droplet"), JsonConverter(typeof (VcapGuidConverter))]
        public Guid DropletID { get; set; }
    }
}