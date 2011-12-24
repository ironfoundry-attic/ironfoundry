namespace IronFoundry.Types
{
    using System;
    using Newtonsoft.Json;

     public class Usage
     {
        [JsonProperty(PropertyName="time")]
        public DateTime CurrentTime { get; set; }

        [JsonProperty(PropertyName="cpu")]
        public float CpuTime { get; set; }

        [JsonProperty(PropertyName="mem")]
        public float MemoryUsage { get; set; }

        [JsonProperty(PropertyName="disk")]
        public float DiskUsage { get; set; }
    }
}