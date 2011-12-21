namespace IronFoundry.Types
{
    using Newtonsoft.Json;

    public class StatInfo : EntityBase
    {
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "stats")]
        public Stats Stats { get; set; }

        [JsonIgnore]
        public int ID { get; set; }
    }
}