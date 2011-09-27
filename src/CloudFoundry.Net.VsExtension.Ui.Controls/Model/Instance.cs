using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    public class AInstance
    {
        public AInstance()
        {
        
        }

        public int ID { get; set; }
        public string Host { get; set; }
        public decimal CpuPercent { get; set; }
        public int Memory { get; set; }
        public int Disk { get; set; }
        public TimeSpan Uptime { get; set; }
        public DirectoryInfo Files { get; set; }
        public Application Parent { get; set; }
    }
}
