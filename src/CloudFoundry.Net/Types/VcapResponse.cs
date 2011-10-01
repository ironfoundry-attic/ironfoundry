namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public class VcapResponse : JsonBase
    {
        [JsonProperty(PropertyName = "code")]
        public int Code
        {
            get; set;
        }

        [JsonProperty(PropertyName = "description")]
        public string Description
        {
            get; set;
        }
    }
}