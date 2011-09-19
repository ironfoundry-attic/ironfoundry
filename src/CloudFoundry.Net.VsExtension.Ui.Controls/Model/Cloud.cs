using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    public class Cloud
    {
        public Cloud() {
            Applications = new ObservableCollection<Application>();
            ID = Guid.NewGuid();
        }

        public Guid ID { get; set; }
        public string ServerName { get; set; }
        public string HostName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public bool Connected { get; set; }
        public int TimeoutStart { get; set; }
        public int TimeoutStop { get; set; }
        public ObservableCollection<Application> Applications { get; set; }
        public ObservableCollection<Service> Services { get; set; }
    }
}
