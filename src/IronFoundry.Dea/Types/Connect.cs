namespace IronFoundry.Dea.Types
{
    using Newtonsoft.Json;

    public class Connect : Message
    {
        [JsonProperty(PropertyName = "verbose")]
        public bool Verbose { get; set; }

        [JsonProperty(PropertyName = "pedantic")]
        public bool Pedantic { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }
    }
}