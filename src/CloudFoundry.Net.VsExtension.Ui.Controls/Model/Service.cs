using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    public class AService
    {
        public AService()
        {

        }

        public string Name { get; set; }
        public string ServiceType { get; set; }
        public string Vendor { get; set; }
        public string Version { get; set; }
        public Cloud Parent { get; set; }
        
    }
}
