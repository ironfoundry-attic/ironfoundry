namespace IronFoundry.Ui.Controls.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using GalaSoft.MvvmLight.Command;
    using IronFoundry.Ui.Controls.Model;
    using IronFoundry.Ui.Controls.Mvvm;
    using IronFoundry.Ui.Controls.Utilities;

    public class ManageCloudsViewModel : DialogViewModel
    {
        private static readonly Guid ironFoundryID = new Guid("1A78AEF6-68E4-43D7-BE78-94289285A1CA");
        private static readonly Guid cloudFoundryID = new Guid("6913D1B2-5D6D-4640-B1AA-4E24D5F129CC");
        private static readonly ManageCloudsData ironFoundryDefault =
            new ManageCloudsData(ironFoundryID) { ServerName = "Iron Foundry", ServerUrl = "http://api.gofoundry.net" };

        private static readonly ManageCloudsData[] defaultClouds = new[]
            {
                ironFoundryDefault,
                new ManageCloudsData(cloudFoundryID) { ServerName = "Cloud Foundry", ServerUrl = "http://api.cloudfoundry.com" },
                new ManageCloudsData { ServerName = "Local Vcap", ServerUrl = "http://api.vcap.me" },
                new ManageCloudsData { ServerName = "Micro Cloud", ServerUrl = "http://api.{username}.cloudfoundry.me", IsMicro = true },
            };

        private readonly ObservableCollection<ManageCloudsData> cloudData = new ObservableCollection<ManageCloudsData>();

        private ManageCloudsData selectedCloud;
        private bool isSelectedCloudAccountValid = false;
        private readonly RelayCommand validateAccountCommand;

        public ManageCloudsViewModel() : base(Messages.PreferencesDialogResult)
        {
            validateAccountCommand = new RelayCommand(ValidateAccount, CanValidate);
        }

        public RelayCommand ValidateAccountCommand { get { return validateAccountCommand; } }

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

        public ManageCloudsData AddCloud()
        {
            var newCloud = new ManageCloudsData(Guid.NewGuid())
            {
                ServerName = "New Server",
                ServerUrl = "http://api.vcap.me",
                Email = "unknown@unset.com",
            };
            AddCloud(newCloud);
            return newCloud;
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
                if (SetValue(ref selectedCloud, value, "SelectedCloud"))
                {
                    IsSelectedCloudAccountValid = false;
                }
            }
        }

        public bool IsSelectedCloudAccountValid
        {
            get { return isSelectedCloudAccountValid; }
            set
            {
                SetValue(ref isSelectedCloudAccountValid, value, "IsSelectedCloudAccountValid");
            }
        }

        protected override void OnConfirmed(CancelEventArgs args)
        {
            foreach (ManageCloudsData mcd in CloudData)
            {
                CloudUpdate cloudUpdate = mcd.GetUpdateData();
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
            SelectedCloud = cloudData.FirstOrDefault();
            if (null == SelectedCloud)
            {
                AddCloud(ironFoundryDefault);
                SelectedCloud = ironFoundryDefault;
            }
        }

        private void AddCloud(ManageCloudsData cloud)
        {
            if (false == cloudData.Contains(cloud))
            {
                cloudData.Add(cloud);
            }
        }

        private bool CanValidate()
        {
            return null != SelectedCloud && SelectedCloud.CanValidate();
        }

        private void ValidateAccount()
        {
            ErrorMessage = string.Empty;
            ManageCloudsData selected = SelectedCloud;
            selected.IsAccountValid = false;
            ProviderResponse<bool> result = provider.ValidateAccount(selected.ServerUrl, selected.Email, selected.Password);
            if (result.Response)
            {
                IsSelectedCloudAccountValid =
                    selected.IsAccountValid = true;
            }
            else
            {
                IsSelectedCloudAccountValid =
                    selected.IsAccountValid = false;
                ErrorMessage = result.Message;
            }
        }
    }

    public class ManageCloudsData : ViewModelBaseEx, IEquatable<ManageCloudsData>
    {
        private readonly Guid id    = Guid.Empty;
        private string serverName   = null;
        private string serverUrl    = null;
        private string email        = null;
        private string password     = null;
        private bool isAccountValid = false;

        private bool isMicro = false;

        public ManageCloudsData() { }

        public ManageCloudsData(Guid id)
        {
            this.id = id;
        }

        public Guid ID { get { return id; } }

        public bool IsMicro
        {
            get { return isMicro; }
            set { isMicro = value; }
        }

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

        public bool IsAccountValid
        {
            get { return isAccountValid; }
            set { SetValue(ref isAccountValid, value, "IsAccountValid"); }
        }

        public CloudUpdate GetUpdateData()
        {
            Guid id = this.ID;
            if (default(Guid) == id)
            {
                id = Guid.NewGuid();
            }
            return new CloudUpdate(id, this.ServerUrl, this.ServerName, this.Email, this.Password);
        }

        public bool CanValidate()
        {
            return false == Email.IsNullOrWhiteSpace() && false == Password.IsNullOrWhiteSpace();
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