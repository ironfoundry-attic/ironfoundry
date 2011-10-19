using System;
using System.Collections.ObjectModel;
using CloudFoundry.Net.Extensions;

namespace CloudFoundry.Net.Types
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
                    cloud.ClearAvailableServices();
                    cloud.IsConnected = false;
                    cloud.IsDisconnected = true;
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
