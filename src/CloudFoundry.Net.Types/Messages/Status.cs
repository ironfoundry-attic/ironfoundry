namespace CloudFoundry.Net.Types.Messages
{
    using Newtonsoft.Json;

    public class Status : Hello
    {
        [JsonProperty(PropertyName = "max_memory")]
        public uint MaxMemory { get; set; }

        [JsonProperty(PropertyName = "reserved_memory")]
        public uint ReservedMemory { get; set; }

        [JsonProperty(PropertyName = "used_memory")]
        public uint UsedMemory { get; set; }

        [JsonProperty(PropertyName = "num_clients")]
        public uint NumClients { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }
    }
}