namespace IronFoundry.Dea.Types
{
    using System;
    using IronFoundry.Dea.JsonConverters;
    using Newtonsoft.Json;

    public class Usage
    {
        [JsonIgnore]
        public ulong TotalCpuTicks { get; set; }

        [JsonProperty(PropertyName="time"), JsonConverter(typeof(VcapDateTimeConverter))]
        public DateTime Time { get; set; }

        [JsonProperty(PropertyName="cpu")]
        public float Cpu { get; set; }

        [JsonProperty(PropertyName="mem")]
        public ulong MemoryUsageKB { get; set; }

        [JsonProperty(PropertyName="disk")]
        public ulong DiskUsageB { get; set; }
    }
}