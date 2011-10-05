namespace CloudFoundry.Net.Types
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    [Serializable]
    public class CloudUrl : EntityBase
    {
        private static ObservableCollection<CloudUrl> defaultCloudUrls = new ObservableCollection<CloudUrl>
        {
            new CloudUrl { ServerType = "Local cloud", Url = "http://api.vcap.me", IsConfigurable = false},
            new CloudUrl { ServerType = "Microcloud", Url = "http://api.{mycloud}.cloudfoundry.me", IsConfigurable = true, IsMicroCloud = true },
            new CloudUrl { ServerType = "VMware Cloud Foundry", Url = "https://api.cloudfoundry.com", IsDefault = true },
            new CloudUrl { ServerType = "vmforce", Url = "http://api.alpha.vmforce.com", IsConfigurable = false },
        };

        public string ServerType { get; set; }
        public string Url { get; set; }
        public bool IsConfigurable { get; set; }
        public bool IsRemovable { get; set; }
        public bool IsDefault { get; set; }
        public bool IsMicroCloud { get; set; }

        public static ObservableCollection<CloudUrl> DefaultCloudUrls
        {
            get { return defaultCloudUrls; }
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
