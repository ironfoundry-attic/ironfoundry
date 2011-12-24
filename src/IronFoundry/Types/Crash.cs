namespace IronFoundry.Types
{
    using Newtonsoft.Json;

    public class Crash : EntityBase
    {
        [JsonProperty(PropertyName = "instance")]
        public string Instance { get; private set; }

        [JsonProperty(PropertyName = "since")]
        public int Since { get; private set; }
    }
}
