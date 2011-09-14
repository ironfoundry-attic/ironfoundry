using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    public class Application
    {        
        public Application()
        {
            MappedUrls = new List<string>();
            Instances = new List<Instance>();
            Services = new List<Service>();
        }

        public string Name { get; set; }
        public int MemoryLimit { get; set; }
        public List<string> MappedUrls { get; set; }
        public List<Instance> Instances { get; set; }
        public List<Service> Services { get; set; }

        public int DiskLimit
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public int Cpus
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public Cloud Parent
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public int InstanceCount
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }
        
    }
}
