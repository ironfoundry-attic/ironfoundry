namespace IronFoundry.Ui.Controls.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using IronFoundry.Ui.Controls.Model;
    using IronFoundry.Ui.Controls.Mvvm;
    using IronFoundry.Ui.Controls.Utilities;

    public class ManageCloudsViewModel : DialogViewModel
    {
        private readonly ObservableCollection<ManageCloudsData> cloudData = new ObservableCollection<ManageCloudsData>();
        private ManageCloudsData selectedCloud;

        public ManageCloudsViewModel() : base(Messages.PreferencesDialogResult) { }

        public ObservableCollection<ManageCloudsData> CloudData
        {
            get { return cloudData; }
        }

        public void AddCloud()
        {
            cloudData.Add(new ManageCloudsData { ServerName = "New Server" });
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
                var cloudUpdate = new CloudUpdate(mcd.ServerUri, mcd.ServerName, mcd.Email, mcd.Password);
                provider.SaveOrUpdate(cloudUpdate);
            }
            provider.SaveChanges();
        }

        protected override void OnProviderRetrieved()
        {
            cloudData.Clear();
            foreach (Types.Cloud cloud in provider.Clouds)
            {
                cloudData.Add(new ManageCloudsData
                {
                    ServerName = cloud.ServerName,
                    ServerUrl  = cloud.Url,
                    // TODO Removable  = false == cloud.IsDefault,
                });
            }
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
    }

    public class ManageCloudsData : ViewModelBaseEx
    {
        private string serverName = null;
        private string serverUrl  = null;
        private string email      = null;
        private string password   = null;
        private bool removable = false;

        public string ServerName
        {
            get { return serverName; }
            set { SetValue(ref serverName, value, "ServerName"); }
        }

        public Uri ServerUri
        {
            get { return new Uri(ServerUrl); }
        }

        public string ServerUrl
        {
            get { return serverUrl; }
            set { SetValue(ref serverUrl, value, "ServerUrl"); }
        }

        public string Email
        {
            get { return email; }
            set { SetValue(ref email, value, "Email"); }
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
    }
}