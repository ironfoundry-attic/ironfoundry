using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    public class ACloud
    {
        public ACloud() {
            Applications = new ObservableCollection<AApplication>();            
        }
        
        public string ServerName { get; set; }
        public string HostName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public bool Connected { get; set; }
        public int TimeoutStart { get; set; }
        public int TimeoutStop { get; set; }
        public string AccessToken { get; set; }
        public ObservableCollection<AApplication> Applications { get; set; }
        public ObservableCollection<Service> Services { get; set; }
    }
}
