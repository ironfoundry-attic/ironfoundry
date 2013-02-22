using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Extensions;

    [Serializable, Obsolete]
    public class CloudUrl : EntityBase, IMergeable<CloudUrl>
    {
        private static readonly SafeObservableCollection<CloudUrl> defaultCloudUrls = new SafeObservableCollection<CloudUrl>
        {
            new CloudUrl {ServerName = "Iron Foundry", Url = "http://api.ironfoundry.me", IsDefault = true, IsConfigurable = false},
            new CloudUrl {ServerName = "Local cloud", Url = "http://api.vcap.me", IsConfigurable = false},
            new CloudUrl
            {ServerName = "Microcloud", Url = "http://api.{mycloud}.cloudfoundry.me", IsConfigurable = true, IsMicroCloud = true},
            new CloudUrl
            {ServerName = "VMWare Cloud Foundry", Url = "https://api.cloudfoundry.com", IsConfigurable = false, IsDefault = false},
        };

        private bool isConfigurable;
        private bool isDefault;
        private bool isMicroCloud;
        private bool isRemovable;
        private string serverName;
        private string url;

        public string ServerName
        {
            get { return serverName; }
            set
            {
                serverName = value;
                RaisePropertyChanged("ServerName");
            }
        }

        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                RaisePropertyChanged("Url");
            }
        }

        public bool IsConfigurable
        {
            get { return isConfigurable; }
            set
            {
                isConfigurable = value;
                RaisePropertyChanged("IsConfigurable");
            }
        }

        public bool IsRemovable
        {
            get { return isRemovable; }
            set
            {
                isRemovable = value;
                RaisePropertyChanged("IsRemovable");
            }
        }

        public bool IsDefault
        {
            get { return isDefault; }
            set
            {
                isDefault = value;
                RaisePropertyChanged("IsDefault");
            }
        }

        public bool IsMicroCloud
        {
            get { return isMicroCloud; }
            set
            {
                isMicroCloud = value;
                RaisePropertyChanged("IsMicroCloud");
            }
        }

        public static SafeObservableCollection<CloudUrl> DefaultCloudUrls
        {
            get { return defaultCloudUrls; }
        }

        #region IMergeable<CloudUrl> Members

        public void Merge(CloudUrl obj)
        {
            Url = obj.Url;
            IsConfigurable = obj.IsConfigurable;
            IsRemovable = obj.IsRemovable;
            IsDefault = obj.IsDefault;
            IsMicroCloud = obj.IsMicroCloud;
        }

        #endregion
    }

    public class CloudUrlEqualityComparer : IEqualityComparer<CloudUrl>
    {
        #region IEqualityComparer<CloudUrl> Members

        public bool Equals(CloudUrl c1, CloudUrl c2)
        {
            return c1.ServerName.Equals(c2.ServerName);
        }

        public int GetHashCode(CloudUrl c)
        {
            return c.ServerName.GetHashCode();
        }

        #endregion
    }
}