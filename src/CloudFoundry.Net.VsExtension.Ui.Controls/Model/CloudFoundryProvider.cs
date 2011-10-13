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
    using System;

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
                message.Execute(this);
        }

        private void CloudChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "AccessToken":
                case "Email":
                case "ServerName":
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

        public ProviderResponse<Cloud> Connect(Cloud cloud)
        {
            ProviderResponse<Cloud> response = null;
            Cloud local = cloud.DeepCopy();
            IVcapClient client = new VcapClient(local);

            try
            {
                VcapClientResult result = client.Login();
                if (!result.Success)
                    throw new Exception(result.Message);
                local.AccessToken = client.CurrentToken;
                var applications = client.GetApplications();
                if (null != applications)
                {
                    local.Applications.Synchronize(new ObservableCollection<Application>(applications), new ApplicationEqualityComparer());
                    var provisionedServices = client.GetProvisionedServices();
                    var availableServices = client.GetSystemServices();
                    local.Services.Synchronize(new ObservableCollection<ProvisionedService>(provisionedServices), new ProvisionedServiceEqualityComparer());
                    local.AvailableServices.Synchronize(new ObservableCollection<SystemService>(availableServices), new SystemServiceEqualityComparer());
                }
                response = new ProviderResponse<Cloud>(local, string.Empty);
            }
            catch (Exception ex)
            {
                response = new ProviderResponse<Cloud>(null, ex.Message);
            }

            return response;
           
        }

        public Cloud Disconnect(Cloud cloud)
        {
            cloud.AccessToken = string.Empty;
            cloud.ClearApplications();
            cloud.ClearServices();
            cloud.ClearAvailableServices();
            return cloud;
        }

        public ProviderResponse<IEnumerable<StatInfo>> GetStats(Application app, Cloud cloud)
        {
            ProviderResponse<IEnumerable<StatInfo>> response = null;
            try
            {
                IVcapClient client = new VcapClient(cloud);
                var stats = client.GetStats(app);
                response = new ProviderResponse<IEnumerable<StatInfo>>(stats, string.Empty);
            }
            catch (Exception ex)
            {
                response = new ProviderResponse<IEnumerable<StatInfo>>(null, ex.Message);
            }
            return response;
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
            return client.GetApplication(argApp.Name);
        }

        public VcapClientResult CreateService(Cloud argCloud, string serviceName, string provisionedServiceName)
        {
            IVcapClient client = new VcapClient(argCloud);
            return client.CreateService(serviceName,provisionedServiceName);
        }

        public ObservableCollection<ProvisionedService> GetProvisionedServices(Cloud argCloud)
        {
            IVcapClient client = new VcapClient(argCloud);
            return new ObservableCollection<ProvisionedService>(client.GetProvisionedServices());
        }

        public VcapClientResult ChangePassword(Cloud argCloud, string newPassword)
        {
            IVcapClient client = new VcapClient(argCloud);
            return client.ChangePassword(newPassword);
        }
    }

    public class ProviderResponse<T>
    {
        public T Response { get; set; }
        public string Message { get; set; }
        
        public ProviderResponse(T response, string message)
        {
            Response = response;
            Message = message;
        }
    }
}