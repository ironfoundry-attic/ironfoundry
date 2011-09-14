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
        static Random rnd = new Random();

        public ApplicationTreeViewItemViewModel(Application application, CloudTreeViewItemViewModel parentCloud) : base(parentCloud, true)
        {
            this.application = application;
        }

        public string Name
        {
            get { return this.application.Name; }
        }

        protected override void LoadChildren()
        {
            application.Instances.AddRange(new Instance[] { 
                new Instance() {
                    Host = GetRandomIP()  
                },
                new Instance() {
                    Host = GetRandomIP()
                },
                new Instance() {
                    Host = GetRandomIP()
                }
            });
            foreach (Instance instance in application.Instances)
                base.Children.Add(new InstanceTreeViewItemViewModel(instance, this));
        }

        private string GetRandomIP()
        {            
            return string.Format("{0}.{1}.{2}.{3}",
                          Convert.ToInt32(rnd.NextDouble() * 255.0),
                          Convert.ToInt32(rnd.NextDouble() * 255.0),
                          Convert.ToInt32(rnd.NextDouble() * 255.0),
                          Convert.ToInt32(rnd.NextDouble() * 255.0)
                          );
        }
    }
}
