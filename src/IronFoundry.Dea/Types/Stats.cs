namespace IronFoundry.Dea.Types
{
    using System;
    using Newtonsoft.Json;

    public class Stats : Message
    {
        public Stats() { }

        public Stats(Instance instance)
        {
            TimeSpan uptimeSpan = DateTime.Now - instance.StartDate;

            Name      = instance.Name;
            Host      = instance.Host;
            Port      = instance.Port;
            Uptime    = uptimeSpan.TotalSeconds;
            Uris      = instance.Uris;
            MemQuotaBytes  = instance.MemQuotaBytes;
            DiskQuotaBytes = instance.DiskQuotaBytes;
            FdsQuota  = instance.FdsQuota;
            Cores     = (uint)Environment.ProcessorCount; // TODO
            Usage     = instance.MostRecentUsage;
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
        public uint MemQuotaBytes { get; set; }

        [JsonProperty(PropertyName = "disk_quota")]
        public uint DiskQuotaBytes { get; set; }

        [JsonProperty(PropertyName = "fds_quota")]
        public uint FdsQuota { get; set; }

        [JsonProperty(PropertyName = "cores")]
        public uint Cores { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public Usage Usage { get; set; }

        public override bool CanPublishWithSubject(string subject)
        {
            return false == subject.IsNullOrWhiteSpace();
        }
    }
}
