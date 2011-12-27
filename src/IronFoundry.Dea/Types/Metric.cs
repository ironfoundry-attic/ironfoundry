namespace IronFoundry.Dea.Types
{
    using Newtonsoft.Json;

    public class Metric : EntityBase
    {
        public Metric()
        {
            UsedMemory = ReservedMemory = UsedDisk = UsedCpu = 0;
        }

        [JsonProperty(PropertyName="used_memory")]
        public ulong UsedMemory { get; set; }

        [JsonProperty(PropertyName="reserved_memory")]
        public ulong ReservedMemory { get; set; }

        [JsonProperty(PropertyName="used_disk")]
        public ulong UsedDisk { get; set; }

        [JsonProperty(PropertyName="used_cpu")]
        public ulong UsedCpu { get; set; }
    }
}