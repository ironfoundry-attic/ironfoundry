using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    public class CloudUrl
    {
        public string ServerType { get; set; }
        public string Url { get; set; }
        public bool IsConfigurable { get; set; }
        public bool IsRemovable { get; set; }
        public bool IsDefault { get; set; }
        public bool IsMicroCloud { get; set; }
    }
}
