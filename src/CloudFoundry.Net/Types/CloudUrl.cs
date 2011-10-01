using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;

namespace CloudFoundry.Net.Types
{
    [Serializable]
    public class CloudUrl : EntityBase
    {
        public string ServerType { get; set; }
        public string Url { get; set; }
        public bool IsConfigurable { get; set; }
        public bool IsRemovable { get; set; }
        public bool IsDefault { get; set; }
        public bool IsMicroCloud { get; set; }

        public static ObservableCollection<CloudUrl> GetDefaultCloudUrls()
        {
            ObservableCollection<CloudUrl> cloudUrls = new ObservableCollection<CloudUrl>() {
                new CloudUrl() { ServerType = "Local cloud", Url = "http://api.vcap.me", IsConfigurable = false},
                new CloudUrl() { ServerType = "Microcloud", Url = "http://api.{mycloud}.cloudfoundry.me", IsConfigurable = true, IsMicroCloud = true },
                new CloudUrl() { ServerType = "VMware Cloud Foundry", Url = "https://api.cloudfoundry.com", IsDefault = true },
                new CloudUrl() { ServerType = "vmforce", Url = "http://api.alpha.vmforce.com", IsConfigurable = false}
            };
            return cloudUrls;
        }
    }

    public class CloudUrlEqualityComparer : IEqualityComparer<CloudUrl>
    {
        public bool Equals(CloudUrl c1, CloudUrl c2)
        {
            return c1.ServerType.Equals(c2.ServerType);
        }

        public int GetHashCode(CloudUrl c)
        {
            return c.ServerType.GetHashCode();
        }
    }
}
