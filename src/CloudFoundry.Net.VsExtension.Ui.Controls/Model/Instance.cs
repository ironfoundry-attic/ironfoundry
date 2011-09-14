using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    public class Instance
    {
        public Instance()
        {
        
        }

        public int ID { get; set; }
        public string Host { get; set; }
        public decimal CpuPercent { get; set; }
        public int Memory { get; set; }
        public int Disk { get; set; }
        public TimeSpan Uptime { get; set; }

        public DirectoryInfo Files
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public Application Parent
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }
    }
}
