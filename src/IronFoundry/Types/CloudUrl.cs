namespace IronFoundry.Types
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    [Serializable, Obsolete]
    public class CloudUrl : EntityBase, IMergeable<CloudUrl>
    {
        private string serverName;
        private string url;
        private bool isConfigurable;
        private bool isRemovable;
        private bool isDefault;
        private bool isMicroCloud;
        
        private static SafeObservableCollection<CloudUrl> defaultCloudUrls = new SafeObservableCollection<CloudUrl>
        {
            new CloudUrl { ServerName = "Iron Foundry", Url = "http://api.gofoundry.net", IsDefault = true, IsConfigurable = false },
            new CloudUrl { ServerName = "Local cloud", Url = "http://api.vcap.me", IsConfigurable = false},
            new CloudUrl { ServerName = "Microcloud", Url = "http://api.{mycloud}.cloudfoundry.me", IsConfigurable = true, IsMicroCloud = true },
            new CloudUrl { ServerName = "VMWare Cloud Foundry", Url = "https://api.cloudfoundry.com", IsConfigurable = false, IsDefault = false },
        };

        public string ServerName
        {
            get { return this.serverName; }
            set { this.serverName = value; RaisePropertyChanged("ServerName"); }
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
            this.Url            = obj.Url;
            this.IsConfigurable = obj.IsConfigurable;
            this.IsRemovable    = obj.IsRemovable;
            this.IsDefault      = obj.IsDefault;
            this.IsMicroCloud   = obj.IsMicroCloud;
        }
    }

    public class CloudUrlEqualityComparer : IEqualityComparer<CloudUrl>
    {
        public bool Equals(CloudUrl c1, CloudUrl c2)
        {
            return c1.ServerName.Equals(c2.ServerName);
        }

        public int GetHashCode(CloudUrl c)
        {
            return c.ServerName.GetHashCode();
        }
    }
}