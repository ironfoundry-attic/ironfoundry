namespace CloudFoundry.Net.Types.Messages
{
    using Entities;
    using Newtonsoft.Json;

    public class Discover : JsonBase
    {
        [JsonProperty(PropertyName = "droplet")]
        public int Droplet { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "runtime")]
        public string Runtime { get; set; }

        [JsonProperty(PropertyName = "sha")]
        public string Sha { get; set; }

        [JsonProperty(PropertyName = "limits")]
        public Limits Limits { get; set; }
    }
}