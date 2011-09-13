namespace CloudFoundry.Net.Types.Entities
{
    using Newtonsoft.Json;

    public class Stats : JsonBase
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
        public int MemQuota { get; set; }

        [JsonProperty(PropertyName = "disk_quota")]
        public int DiskQuota { get; set; }

        [JsonProperty(PropertyName = "fds_quota")]
        public int FdsQuota { get; set; }

        [JsonProperty(PropertyName = "cores")]
        public int Cores { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public int Usage { get; set; }
    }
}