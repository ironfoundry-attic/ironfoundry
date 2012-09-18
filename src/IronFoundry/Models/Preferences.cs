using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronFoundry.Models
{
    using Extensions;

    [Serializable, Obsolete]
    public class Preferences
    {
        private SafeObservableCollection<CloudUrl> cloudUrls;
        private SafeObservableCollection<Cloud> clouds;

        public SafeObservableCollection<Cloud> Clouds
        {
            get { return clouds; }
            set
            {
                clouds = value.DeepCopy();
                foreach (Cloud cloud in clouds)
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
            get { return cloudUrls; }
            set { cloudUrls = value.DeepCopy(); }
        }
    }
}