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
        readonly Instance instance;

        public InstanceTreeViewItemViewModel(Instance instance, ApplicationTreeViewItemViewModel parentApplication) : base(parentApplication,false)
        {
            this.instance = instance;
        }

        public string Host
        {
            get { return this.instance.Host; }
        }
    }
}
