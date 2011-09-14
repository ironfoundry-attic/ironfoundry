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

        public CloudTreeViewItemViewModel(Cloud cloud) : base(null,true)
        {
            this.cloud = cloud;
        }

        public string ServerName
        {
            get { return this.cloud.ServerName; }
        }

        protected override void LoadChildren()
        {
            cloud.Applications.AddRange( new Application[] { 
                new Application() {
                    Name = "Application " + Guid.NewGuid().ToString("D")   
                },
                new Application() {
                    Name = "Application " + Guid.NewGuid().ToString("D")
                },
                new Application() {
                    Name = "Application " + Guid.NewGuid().ToString("D")
                }
            });
            foreach (Application app in cloud.Applications)
                base.Children.Add(new ApplicationTreeViewItemViewModel(app,this));
        }
    }
}
