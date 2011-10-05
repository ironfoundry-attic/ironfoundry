namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using CloudFoundry.Net.Types;
    using CloudFoundry.Net.Vmc;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight.Messaging;

    public class CloudFoundryProvider
    {
        private PreferencesProvider preferencesProvider;
        public ObservableCollection<Cloud> Clouds { get; private set;}
        public ObservableCollection<CloudUrl> CloudUrls { get; private set; }
        public event NotifyCollectionChangedEventHandler CloudsChanged;

        public CloudFoundryProvider(PreferencesProvider preferencesProvider)
        {
            this.preferencesProvider = preferencesProvider;
            var preferences          = preferencesProvider.LoadPreferences();
            this.Clouds              = preferences.Clouds.DeepCopy();            
            this.CloudUrls           = preferences.CloudUrls.DeepCopy();

            this.Clouds.CollectionChanged += Clouds_CollectionChanged;
            foreach (var cloud in Clouds)
                cloud.PropertyChanged += CloudChanged;

            Messenger.Default.Register<NotificationMessageAction<CloudFoundryProvider>>(this, ProcessCloudFoundryProviderMessage);
        }
        
        private void ProcessCloudFoundryProviderMessage(NotificationMessageAction<CloudFoundryProvider> message)
        {
            if (message.Notification.Equals(Messages.GetCloudFoundryProvider))
            {
                message.Execute(this);
            }
        }

        private void CloudChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "AccessToken":
                case "Email":
                case "ServerName":
                case "HostName":
                case "Url":
                case "TimeoutStart":
                case "TimeoutStop":
                case "ID":
                case "Password":
                case "IsConnected":
                case "IsDisconnected":
                    preferencesProvider.SavePreferences(new Preferences() { Clouds = this.Clouds, CloudUrls = this.CloudUrls });
                    break;
                default:
                    break;
            }
        }

        private void Clouds_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.CloudsChanged != null)
                this.CloudsChanged(sender, e);
            preferencesProvider.SavePreferences(new Preferences() { Clouds = this.Clouds, CloudUrls = this.CloudUrls });
        }

        public void SaveChanges()
        {
            preferencesProvider.SavePreferences(new Preferences() { Clouds = this.Clouds, CloudUrls = this.CloudUrls });
        }

        public Cloud Connect(Cloud cloud)
        {            
            Cloud local = cloud.DeepCopy();
            IVcapClient client = new VcapClient(local);

            VcapClientResult result = client.Login();
            if (result.Success)
            {
                local.AccessToken = client.CurrentToken;
                var applications = client.ListApps();
                if (null != applications)
                {
                    local.Applications.Synchronize(new ObservableCollection<Application>(applications), new ApplicationEqualityComparer());
                    var provisionedServices = client.GetProvisionedServices();
                    local.Services.Synchronize(new ObservableCollection<ProvisionedService>(provisionedServices), new ProvisionedServiceEqualityComparer());
                }
                return local;
            }
            else
            {
                return null;
            }
        }

        public Cloud Disconnect(Cloud cloud)
        {
            cloud.AccessToken = string.Empty;
            cloud.Applications.Clear();
            cloud.Services.Clear();
            return cloud;
        }

        public IEnumerable<StatInfo> GetStats(Application app, Cloud cloud)
        {
            IVcapClient client = new VcapClient(cloud);
            return client.GetStats(app);
        }

        public VcapResponse UpdateApplication(Application app, Cloud cloud)
        {
            IVcapClient client = new VcapClient(cloud);
            return client.UpdateApplication(app);
        }

        public void Start(Application app, Cloud cloud)
        {
            IVcapClient client = new VcapClient(cloud);
            client.Start(app);
        }

        public void Stop(Application argApp, Cloud argCloud)
        {
            IVcapClient client = new VcapClient(argCloud);
            client.Stop(argApp);
        }

        public void Restart(Application argApp, Cloud argCloud)
        {
            IVcapClient client = new VcapClient(argCloud);
            client.Restart(argApp);
        }

        public void UpdateAndRestart(Application argApp, Cloud argCloud)
        {
            IVcapClient client = new VcapClient(argCloud);
            client.UpdateApplication(argApp);
            client.Restart(argApp);
        }

        public Application GetApplication(Application argApp, Cloud argCloud)
        {
            IVcapClient client = new VcapClient(argCloud);
            return client.GetAppInfo(argApp.Name);
        }
    }
}