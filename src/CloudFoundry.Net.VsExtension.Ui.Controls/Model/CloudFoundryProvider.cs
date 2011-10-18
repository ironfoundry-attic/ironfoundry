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
            ProviderResponse<Cloud> response = new ProviderResponse<Cloud>();
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
                response.Response = local;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
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

        public ProviderResponse<bool> ValidateAccount(Cloud cloud)
        {
            ProviderResponse<bool> response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                var vcapResponse = client.Login();
                if (vcapResponse != null && 
                    !vcapResponse.Success && 
                    !String.IsNullOrEmpty(vcapResponse.Message))
                    throw new Exception(vcapResponse.Message);
                response.Response = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<IEnumerable<StatInfo>> GetStats(Application app, Cloud cloud)
        {
            ProviderResponse<IEnumerable<StatInfo>> response = new ProviderResponse<IEnumerable<StatInfo>>();
            try
            {
                IVcapClient client = new VcapClient(cloud);                
                response.Response = client.GetStats(app);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<bool> UpdateApplication(Application app, Cloud cloud)
        {
            ProviderResponse<bool> response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                var vcapResponse = client.UpdateApplication(app);
                if (vcapResponse != null && !String.IsNullOrEmpty(vcapResponse.Description))
                    throw new Exception(vcapResponse.Description);
                response.Response = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<bool> Start(Application app, Cloud cloud)
        {
            ProviderResponse<bool> response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                client.Start(app);                
                response.Response = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<bool> Stop(Application app, Cloud cloud)
        {
            ProviderResponse<bool> response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                client.Stop(app);
                response.Response = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<bool> Restart(Application app, Cloud cloud)
        {
            ProviderResponse<bool> response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                client.Restart(app);
                response.Response = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;            
        }

        public ProviderResponse<bool> Delete(Application app, Cloud cloud)
        {
            ProviderResponse<bool> response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                client.Delete(app.Name);
                response.Response = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<Application> GetApplication(Application app, Cloud cloud)
        {
            ProviderResponse<Application> response = new ProviderResponse<Application>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                response.Response = client.GetApplication(app.Name);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<bool> CreateService(Cloud cloud, string serviceName, string provisionedServiceName)
        {
            ProviderResponse<bool> response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                var vcapResult = client.CreateService(serviceName, provisionedServiceName);
                if (!vcapResult.Success)
                    throw new Exception(vcapResult.Message);
                response.Response = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;            
        }

        public ProviderResponse<ObservableCollection<ProvisionedService>> GetProvisionedServices(Cloud cloud)
        {
            ProviderResponse<ObservableCollection<ProvisionedService>> response = new ProviderResponse<ObservableCollection<ProvisionedService>>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                response.Response = new ObservableCollection<ProvisionedService>(client.GetProvisionedServices());
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<bool> ChangePassword(Cloud cloud, string newPassword)
        {
            ProviderResponse<bool> response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                var vcapResult = client.ChangePassword(newPassword);
                if (!vcapResult.Success)
                    throw new Exception(vcapResult.Message);
                response.Response = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;   
        }

        public ProviderResponse<bool> RegisterAccount(Cloud cloud,string email, string password)
        {
            ProviderResponse<bool> response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                var vcapResult = client.AddUser(email,password);
                if (!vcapResult.Success)
                    throw new Exception(vcapResult.Message);
                response.Response = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response; 
        }

        public ProviderResponse<ObservableCollection<StatInfo>> GetStats(Cloud cloud, Application application)
        {
            ProviderResponse<ObservableCollection<StatInfo>> response = new ProviderResponse<ObservableCollection<StatInfo>>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                response.Response = new ObservableCollection<StatInfo>(client.GetStats(application));
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;    
        }

        public ProviderResponse<VcapFilesResult> GetFiles(Cloud cloud, Application application, string path, ushort instanceId)
        {
            ProviderResponse<VcapFilesResult> response = new ProviderResponse<VcapFilesResult>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                var result = client.Files(application.Name, path, instanceId);
                response.Response = result;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<bool> Push(Cloud cloud, string name, string url, ushort instances, string directoryToPushFrom, uint memory, string[] services)
        {
            ProviderResponse<bool> response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                client.Push(name, url, instances, new System.IO.DirectoryInfo(directoryToPushFrom), memory, services);
                response.Response = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<bool> Update(Cloud cloud, Application app, string directoryToPushFrom)
        {
            ProviderResponse<bool> response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                client.Update(app.Name, new System.IO.DirectoryInfo(directoryToPushFrom));
                response.Response = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }

    public class ProviderResponse<T>
    {
        public T Response { get; set; }
        public string Message { get; set; }
        
        public ProviderResponse()
        {
            Response = default(T);
            Message = string.Empty;
        }

        public ProviderResponse(T response, string message)
        {
            Response = response;
            Message = message;
        }
    }
}