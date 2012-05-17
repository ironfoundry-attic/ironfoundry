namespace IronFoundry.Dea.Types
{
    using System;
    using IronFoundry.Dea.JsonConverters;
    using Newtonsoft.Json;

    public class Usage
    {
        [JsonIgnore]
        public long TotalCpuTicks { get; set; }

        [JsonProperty(PropertyName="time"), JsonConverter(typeof(VcapDateTimeConverter))]
        public DateTime Time { get; set; }

        [JsonProperty(PropertyName="cpu")]
        public float Cpu { get; set; }

        [JsonProperty(PropertyName="mem")]
        public long MemoryUsageKB { get; set; }

        [JsonProperty(PropertyName="disk")]
        public long DiskUsageBytes { get; set; }
    }
}