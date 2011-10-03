using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    [Serializable]
    public class Preferences
    {             
        private ObservableCollection<Cloud> clouds;
        private ObservableCollection<CloudUrl> cloudUrls;

        public ObservableCollection<Cloud> Clouds 
        {
            get { return this.clouds; }
            set
            {
                this.clouds = value.DeepCopy();
                foreach (var cloud in this.clouds)
                {
                    cloud.ClearServices();
                    cloud.ClearApplications();
                }
            }
        }
        public ObservableCollection<CloudUrl> CloudUrls
        {
            get { return this.cloudUrls; }
            set
            {
                this.cloudUrls = value.DeepCopy();
                
            }
        }
    }
}
