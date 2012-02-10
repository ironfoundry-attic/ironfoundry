namespace IronFoundry.Ui.Controls.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using IronFoundry.Ui.Controls.Model;
    using IronFoundry.Ui.Controls.Mvvm;
    using IronFoundry.Ui.Controls.Utilities;

    public class ManageCloudsViewModel : DialogViewModel
    {
        private static readonly Guid ironFoundryID = new Guid("1A78AEF6-68E4-43D7-BE78-94289285A1CA");
        private static readonly Guid cloudFoundryID = new Guid("6913D1B2-5D6D-4640-B1AA-4E24D5F129CC");

        private static readonly ManageCloudsData[] defaultClouds = new[]
            {
                new ManageCloudsData(ironFoundryID) { ServerName = "Iron Foundry", ServerUrl = "http://api.gofoundry.net" },
                new ManageCloudsData(cloudFoundryID) { ServerName = "Cloud Foundry", ServerUrl = "http://api.cloudfoundry.com" },
            };

        private readonly ObservableCollection<ManageCloudsData> cloudData = new ObservableCollection<ManageCloudsData>();
        private ManageCloudsData selectedCloud;

        public ManageCloudsViewModel() : base(Messages.PreferencesDialogResult) { }

        public ObservableCollection<ManageCloudsData> CloudData
        {
            get { return cloudData; }
        }

        public IEnumerable<ManageCloudsData> DefaultClouds
        {
            get { return defaultClouds; }
        }

        public void AddDefaultCloud(ManageCloudsData cloud)
        {
            AddCloud(cloud);
        }

        public void AddCloud()
        {
            var newCloud = new ManageCloudsData(Guid.NewGuid())
            {
                ServerName = "New Server",
                ServerUrl = "http://api.gofoundry.net",
                Email = "unknown@unset.com",
            };
            AddCloud(newCloud);
        }

        public void RemoveSelectedCloud()
        {
            if (null != SelectedCloud)
            {
                int idx = cloudData.IndexOf(SelectedCloud);
                cloudData.RemoveAt(idx);
                SelectedCloud = cloudData.FirstOrDefault();
            }
        }

        public ManageCloudsData SelectedCloud
        {
            get { return selectedCloud; }
            set
            {
                if (selectedCloud != value)
                {
                    selectedCloud = value;
                    RaisePropertyChanged("SelectedCloud");
                }
            }
        }

        protected override void OnConfirmed(CancelEventArgs args)
        {
            foreach (ManageCloudsData mcd in CloudData)
            {
                var cloudUpdate = new CloudUpdate(mcd.ID, mcd.ServerUrl, mcd.ServerName, mcd.Email, mcd.Password);
                provider.SaveOrUpdate(cloudUpdate);
            }
            provider.SaveChanges();
        }

        protected override void OnProviderRetrieved()
        {
            cloudData.Clear();
            foreach (Types.Cloud cloud in provider.Clouds)
            {
                AddCloud(new ManageCloudsData(cloud.ID)
                {
                    ServerName = cloud.ServerName,
                    ServerUrl  = cloud.Url,
                    Email      = cloud.Email,
                    Password   = cloud.Password,
                });
            }
        }

        private void AddCloud(ManageCloudsData cloud)
        {
            if (false == cloudData.Contains(cloud))
            {
                cloudData.Add(cloud);
            }
        }
    }

    public class ManageCloudsData : ViewModelBaseEx, IEquatable<ManageCloudsData>
    {
        private readonly Guid id = Guid.Empty;
        private string serverName = null;
        private string serverUrl  = null;
        private string email      = null;
        private string password   = null;
        private bool removable    = false;

        public ManageCloudsData(Guid id)
        {
            this.id = id;
        }

        public Guid ID { get { return id; } }

        public string ServerName
        {
            get { return serverName; }
            set
            {
                if (false == value.IsNullOrWhiteSpace())
                {
                    SetValue(ref serverName, value, "ServerName");
                }
            }
        }

        public string ServerUrl
        {
            get { return serverUrl; }
            set
            {
                if (false == value.IsNullOrWhiteSpace())
                {
                    SetValue(ref serverUrl, value, "ServerUrl");
                }
            }
        }

        public string Email
        {
            get { return email; }
            set
            {
                if (false == value.IsNullOrWhiteSpace())
                {
                    SetValue(ref email, value, "Email");
                }
            }
        }

        public string Password
        {
            get { return password; }
            set { SetValue(ref password, value, "Password"); }
        }

        public bool Removable
        {
            get { return removable; }
            set { removable = value; }
        }

        public bool Equals(ManageCloudsData other)
        {
            if (null == other)
            {
                return false;
            }

            return this.GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ManageCloudsData);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}