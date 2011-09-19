using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    public class Service
    {
        public Service()
        {

        }

        public string Name { get; set; }
        public string ServiceType { get; set; }
        public string Vendor { get; set; }
        public string Version { get; set; }
        public Cloud Parent { get; set; }
        
    }
}
