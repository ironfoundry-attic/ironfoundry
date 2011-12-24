namespace IronFoundry.Dea.Types
{
    using System;
    using JsonConverters;
    using Newtonsoft.Json;

    public class InstanceEntry : EntityBase
    {
        [JsonProperty(PropertyName = "instance_id"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid InstanceID { get; set; }

        [JsonProperty(PropertyName = "instance")]
        public Instance Instance { get; set; }
    }
}