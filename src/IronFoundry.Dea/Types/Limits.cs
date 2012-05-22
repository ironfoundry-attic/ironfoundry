namespace IronFoundry.Dea.Types
{
    using Newtonsoft.Json;

    public class Limits : EntityBase
    {
        [JsonProperty(PropertyName = "mem")]
        public uint MemoryMB { get; set; }

        [JsonProperty(PropertyName = "disk")]
        public uint DiskMB { get; set; }

        [JsonProperty(PropertyName = "fds")]
        public uint FDs { get; set; }
    }
}
