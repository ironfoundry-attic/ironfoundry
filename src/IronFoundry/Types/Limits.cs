namespace IronFoundry.Types
{
    using Newtonsoft.Json;

    public class Limits : EntityBase
    {
        [JsonProperty(PropertyName = "mem")]
        public int Mem { get; set; }

        [JsonProperty(PropertyName = "disk")]
        public int Disk { get; set; }

        [JsonProperty(PropertyName = "fds")]
        public int FDs { get; set; }
    }
}