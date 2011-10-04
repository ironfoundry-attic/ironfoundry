namespace CloudFoundry.Net.Types
{
    using System;
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

    public class Stats : Message
    {
        public Stats()
        {

        }

        public Stats(Instance argInstance, TimeSpan argUptime)
        {
            Name      = argInstance.Name;
            Host      = argInstance.Host;
            Port      = argInstance.Port;
            Uptime    = argUptime.TotalSeconds;
            Uris      = argInstance.Uris;
            MemQuota  = argInstance.MemQuota;
            DiskQuota = argInstance.DiskQuota;
            FdsQuota  = argInstance.FdsQuota;
            Cores     = 1; // TODO
            Usage     = new Usage();
            // TODO Usage = 20
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
        public long MemQuota { get; set; }

        [JsonProperty(PropertyName = "disk_quota")]
        public long DiskQuota { get; set; }

        [JsonProperty(PropertyName = "fds_quota")]
        public long FdsQuota { get; set; }

        [JsonProperty(PropertyName = "cores")]
        public int Cores { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public Usage Usage { get; set; }
    }

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