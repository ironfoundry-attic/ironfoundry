namespace CloudFoundry.Net.Types
{
    using System;
    using Newtonsoft.Json;

    public class StatInfo : JsonBase
    {
        [JsonProperty(PropertyName = "state")]
        public string state;

        [JsonProperty(PropertyName = "stats")]
        public Stats stats;
    }

    public class Stats : Message
    {
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
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "host")]
        public string Host { get; private set; }

        [JsonProperty(PropertyName = "port")]
        public int Port { get; private set; }

        [JsonProperty(PropertyName = "uptime")]
        public double Uptime { get; private set; }

        [JsonProperty(PropertyName = "uris")]
        public string[] Uris { get; private set; }

        [JsonProperty(PropertyName = "mem_quota")]
        public int MemQuota { get; private set; }

        [JsonProperty(PropertyName = "disk_quota")]
        public int DiskQuota { get; private set; }

        [JsonProperty(PropertyName = "fds_quota")]
        public int FdsQuota { get; private set; }

        [JsonProperty(PropertyName = "cores")]
        public int Cores { get; private set; }

        [JsonProperty(PropertyName = "usage")]
        public Usage Usage { get; private set; }
    }

     public class Usage
     {
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