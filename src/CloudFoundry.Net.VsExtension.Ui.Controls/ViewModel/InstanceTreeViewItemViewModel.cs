using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class InstanceTreeViewItemViewModel : TreeViewItemViewModel
    {
        readonly StatInfo statInfo;

        public InstanceTreeViewItemViewModel(StatInfo statInfo, ApplicationTreeViewItemViewModel parentApplication) : base(parentApplication,false)
        {
            this.statInfo = statInfo;
        }

        public string Host
        {
            get { return this.statInfo.stats.Host; }
        }
    }
}
