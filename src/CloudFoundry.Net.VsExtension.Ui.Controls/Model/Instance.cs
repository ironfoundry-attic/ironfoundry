namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    using System;
    using CloudFoundry.Net.Types;

    public class Instance
    {
        public int ID { get; set; }
        public int Cores { get; set; }
        public long MemoryQuota { get; set; }
        public long DiskQuota { get; set; }
        public string Host { get; set; }
        public float Cpu { get; set; }
        public long Memory { get; set; }
        public long Disk { get; set; }
        public TimeSpan Uptime { get; set; }
        public Application Parent { get; set; }
    }
}