namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public class Tag
    {
        [JsonProperty(PropertyName = "framework")]
        public string Framework { get; set; }

        [JsonProperty(PropertyName = "runtime")]
        public string Runtime { get; set; }
    }
}