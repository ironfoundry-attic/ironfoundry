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
        private VcapCredentialManager credentialManager;
        private IVcapClient client;
        public ObservableCollection<Cloud> Clouds { get; private set;}
        public ObservableCollection<CloudUrl> CloudUrls { get; private set; }
        public event NotifyCollectionChangedEventHandler CloudsChanged;

        public CloudFoundryProvider(PreferencesProvider preferencesProvider, IVcapClient client, VcapCredentialManager credentialManager)
        {
            this.client = client;
            this.preferencesProvider = preferencesProvider;
            this.credentialManager = credentialManager;
            var preferences = preferencesProvider.LoadPreferences();
            this.Clouds = preferences.Clouds.DeepCopy();            
            this.CloudUrls = preferences.CloudUrls.DeepCopy();

            this.Clouds.CollectionChanged += Clouds_CollectionChanged;
            foreach (var cloud in Clouds)
                cloud.PropertyChanged += CloudChanged;

            Messenger.Default.Register<NotificationMessageAction<CloudFoundryProvider>>(this, ProcessCloudFoundryProviderMessage);
        }
        
        private void ProcessCloudFoundryProviderMessage(NotificationMessageAction<CloudFoundryProvider> message)
        {
            if (message.Notification.Equals(Messages.GetCloudFoundryProvider))
                message.Execute(this);
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

            VcapClientResult result = client.Login(local);
            if (result.Success)
            {
                local.AccessToken = credentialManager.CurrentToken;
                var applications = client.ListApps(local);
                if (null != applications)
                {
                    local.Applications.Synchronize(new ObservableCollection<Application>(applications), new ApplicationEqualityComparer());
                    var provisionedServices = client.GetProvisionedServices(local);
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
            return client.GetStats(app, cloud);
        }

        public VcapResponse UpdateApplicationSettings(Application app, Cloud cloud)
        {
            return client.UpdateApplicationSettings(app, cloud);
        }

        public void Start(Application app, Cloud cloud)
        {
            client.Start(cloud, app);
        }

        public void Stop(Application app, Cloud cloud)
        {
            client.Stop(cloud, app);
        }

        public void Restart(Application app, Cloud cloud)
        {
            client.RestartApp(app, cloud);
        }

        public void UpdateAndRestart(Application app, Cloud cloud)
        {
            client.UpdateApplicationSettings(app, cloud);
            client.RestartApp(app, cloud);
        }

        public Application GetApplication(Application app, Cloud cloud)
        {
            return client.GetAppInfo(cloud, app.Name);
        }
    }
}
