using IronFoundry.Extensions;

namespace IronFoundry.Types
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    [Serializable]
    public class CloudUrl : EntityBase, IMergeable<CloudUrl>
    {
        private string serverType;
        private string url;
        private bool isConfigurable;
        private bool isRemovable;
        private bool isDefault;
        private bool isMicroCloud;
        
        private static SafeObservableCollection<CloudUrl> defaultCloudUrls = new SafeObservableCollection<CloudUrl>
        {
            new CloudUrl { ServerType = "Local cloud", Url = "http://api.vcap.me", IsConfigurable = false},
            new CloudUrl { ServerType = "Microcloud", Url = "http://api.{mycloud}.cloudfoundry.me", IsConfigurable = true, IsMicroCloud = true },
            new CloudUrl { ServerType = "VMware Cloud Foundry", Url = "https://api.cloudfoundry.com", IsDefault = true },
            new CloudUrl { ServerType = "vmforce", Url = "http://api.alpha.vmforce.com", IsConfigurable = false },
        };

        public string ServerType
        {
            get { return this.serverType; }
            set { this.serverType = value; RaisePropertyChanged("ServerType"); }
        }
        public string Url
        {
            get { return this.url; }
            set { this.url = value; RaisePropertyChanged("Url"); }
        }
        public bool IsConfigurable
        {
            get { return this.isConfigurable; }
            set { this.isConfigurable = value; RaisePropertyChanged("IsConfigurable"); }
        }
        public bool IsRemovable
        {
            get { return this.isRemovable; }
            set { this.isRemovable = value; RaisePropertyChanged("IsRemovable"); }
        }
        public bool IsDefault
        {
            get { return this.isDefault; }
            set { this.isDefault = value; RaisePropertyChanged("IsDefault"); }
        }
        public bool IsMicroCloud
        {
            get { return this.isMicroCloud; }
            set { this.isMicroCloud = value; RaisePropertyChanged("IsMicroCloud"); }
        }

        public static SafeObservableCollection<CloudUrl> DefaultCloudUrls
        {
            get { return defaultCloudUrls; }
        }
    
        public void Merge(CloudUrl obj)
        {
            this.Url = obj.Url;
            this.IsConfigurable = obj.IsConfigurable;
            this.IsRemovable = obj.IsRemovable;
            this.IsDefault = obj.IsDefault;
            this.IsMicroCloud = obj.IsMicroCloud;
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
