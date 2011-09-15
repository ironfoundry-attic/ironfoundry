namespace CloudFoundry.Net.Types
{
    using System;
    using Newtonsoft.Json;

    public class Stats : Message
    {
        public Stats() {
            Usage = new Usage();
        }

        public Stats(Instance argInstance, TimeSpan argSpan)
        {
            Name      = argInstance.Name;
            Host      = argInstance.Host;
            Port      = argInstance.Port;
            Uris      = argInstance.Uris;
            Uptime    = argSpan.TotalSeconds;
            MemQuota  = argInstance.MemQuota;
            DiskQuota = argInstance.DiskQuota;
            FdsQuota  = argInstance.FdsQuota;
            Usage = new Usage();
        }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "host")]
        public string Host { get; set; }

        [JsonProperty(PropertyName = "port")]
        public int Port { get; set; }

        [JsonProperty(PropertyName = "uptime")]
        public double Uptime { get; set; }

        [JsonProperty(PropertyName = "uris")]
        public string[] Uris { get; set; }

        [JsonProperty(PropertyName = "mem_quota")]
        public int MemQuota { get; set; }

        [JsonProperty(PropertyName = "disk_quota")]
        public int DiskQuota { get; set; }

        [JsonProperty(PropertyName = "fds_quota")]
        public int FdsQuota { get; set; }

        [JsonProperty(PropertyName = "cores")]
        public int Cores { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public Usage Usage { get; set; }
    }

     public class Usage {
        
        [JsonProperty(PropertyName="time")]
        DateTime CurrentTime { get; set; }

        [JsonProperty(PropertyName="cpu")]
        float CpuTime { get; set; }

        [JsonProperty(PropertyName="mem")]
        float MemoryUsage { get; set; }

        [JsonProperty(PropertyName="disk")]
        float DisKUsage { get; set; }
    }
}