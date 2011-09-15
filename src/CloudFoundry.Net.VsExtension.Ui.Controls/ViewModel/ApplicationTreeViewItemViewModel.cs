using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class ApplicationTreeViewItemViewModel : TreeViewItemViewModel
    {
        readonly Application application;
        
        public ApplicationTreeViewItemViewModel(Application application, CloudTreeViewItemViewModel parentCloud) : base(parentCloud, false)
        {
            this.application = application;
            foreach (Instance instance in application.Instances)
                base.Children.Add(new InstanceTreeViewItemViewModel(instance, this));
        }

        public string Name
        {
            get { return this.application.Name; }
        }
    }
}
