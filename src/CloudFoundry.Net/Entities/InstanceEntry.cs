namespace CloudFoundry.Net.Types.Entities
{
    using Newtonsoft.Json;

    public class InstanceEntry : JsonBase
    {
        [JsonProperty(PropertyName = "instance_id")]
        public string InstanceID { get; set; }

        [JsonProperty(PropertyName = "instance")]
        public Instance Instance { get; set; }
    }
}