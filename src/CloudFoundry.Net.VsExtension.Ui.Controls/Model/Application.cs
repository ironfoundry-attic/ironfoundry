using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    public class AApplication
    {        
        public AApplication()
        {
            MappedUrls = new ObservableCollection<string>();
            Instances = new ObservableCollection<Instance>();
            Services = new ObservableCollection<Service>();
        }

        public string Name { get; set; }
        public int MemoryLimit { get; set; }
        public ObservableCollection<string> MappedUrls { get; set; }
        public ObservableCollection<Instance> Instances { get; set; }
        public ObservableCollection<Service> Services { get; set; }
        public string State { get; set; }
        public int DiskLimit { get; set; }
        public int Cpus { get; set; }
        public Cloud Parent { get; set; }        
        public int InstanceCount { get; set; }        
        
    }
}
