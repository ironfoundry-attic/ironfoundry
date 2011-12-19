using System;
using System.Collections.ObjectModel;
using CloudFoundry.Net.Extensions;

namespace CloudFoundry.Net.Types
{
    [Serializable]
    public class Preferences
    {             
        private SafeObservableCollection<Cloud> clouds;
        private SafeObservableCollection<CloudUrl> cloudUrls;

        public SafeObservableCollection<Cloud> Clouds 
        {
            get { return this.clouds; }
            set
            {
                this.clouds = value.DeepCopy();
                foreach (var cloud in this.clouds)
                {
                    cloud.Services.Clear();
                    cloud.Applications.Clear();
                    cloud.AvailableServices.Clear();
                    cloud.IsConnected = false;
                    cloud.IsDisconnected = true;
                }
            }
        }
        public SafeObservableCollection<CloudUrl> CloudUrls
        {
            get { return this.cloudUrls; }
            set
            {
                this.cloudUrls = value.DeepCopy();
            }
        }
    }
}
