namespace IronFoundry.Dea.Types
{
    using System;
    using Newtonsoft.Json;

    public class Stats : Message
    {
        public Stats() { }

        public Stats(Instance argInstance)
        {
            TimeSpan uptimeSpan = DateTime.Now - argInstance.StartDate;

            Name      = argInstance.Name;
            Host      = argInstance.Host;
            Port      = argInstance.Port;
            Uptime    = uptimeSpan.TotalSeconds;
            Uris      = argInstance.Uris;
            MemQuota  = argInstance.MemQuota;
            DiskQuota = argInstance.DiskQuota;
            FdsQuota  = argInstance.FdsQuota;
            Cores     = Environment.ProcessorCount; // TODO
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

        public override bool CanPublishWithSubject(string subject)
        {
            return false == subject.IsNullOrWhiteSpace();
        }
    }
}