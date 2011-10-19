namespace CloudFoundry.Net.Types
{
    using System;
    using Newtonsoft.Json;

    public class Stats : Message
    {        

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
}