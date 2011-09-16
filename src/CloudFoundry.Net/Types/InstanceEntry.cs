namespace CloudFoundry.Net.Types
{
    using System;
    using JsonConverters;
    using Newtonsoft.Json;

    public class InstanceEntry : JsonBase
    {
        [JsonProperty(PropertyName = "instance_id"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid InstanceID { get; set; }

        [JsonProperty(PropertyName = "instance")]
        public Instance Instance { get; set; }
    }
}