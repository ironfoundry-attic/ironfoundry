using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class CloudTreeViewItemViewModel : TreeViewItemViewModel
    {
        readonly Cloud cloud;

        public CloudTreeViewItemViewModel(Cloud cloud) : base(null,false)
        {
            this.cloud = cloud;
            foreach (Application app in cloud.Applications)
                base.Children.Add(new ApplicationTreeViewItemViewModel(app, this));
        }

        public string ServerName
        {
            get { return this.cloud.ServerName; }
        }
    }
}
