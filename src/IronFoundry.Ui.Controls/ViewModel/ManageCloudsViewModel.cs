namespace IronFoundry.Ui.Controls.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using IronFoundry.Types;
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
            // provider.CloudUrls.Synchronize(CloudUrls.DeepCopy(), new CloudUrlEqualityComparer());
            // provider.SaveChanges();
        }

        protected override void OnProviderRetrieved()
        {
            cloudData.Clear();
            foreach (CloudUrl cloudUrl in provider.CloudUrls)
            {
                cloudData.Add(new ManageCloudsData
                {
                    ServerName = cloudUrl.ServerName,
                    ServerUrl  = cloudUrl.Url,
                    Removable  = false == cloudUrl.IsDefault,
                });
            }
        }

        public void RemoveSelectedCloud()
        {
            if (null != SelectedCloud)
            {
                int idx = cloudData.IndexOf(SelectedCloud);
                cloudData.RemoveAt(idx);
                SelectedCloud = cloudData[Math.Min(idx, (cloudData.Count - 1))];
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