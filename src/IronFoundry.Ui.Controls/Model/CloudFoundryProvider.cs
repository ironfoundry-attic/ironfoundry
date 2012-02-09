namespace IronFoundry.Ui.Controls.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using GalaSoft.MvvmLight.Messaging;
    using IronFoundry.Types;
    using IronFoundry.Vcap;
    using Utilities;

    public class CloudFoundryProvider : ICloudFoundryProvider
    {
        private PreferencesProvider preferencesProvider;
        public SafeObservableCollection<Cloud> Clouds { get; private set;}
        public SafeObservableCollection<CloudUrl> CloudUrls { get; private set; }
        public event NotifyCollectionChangedEventHandler CloudsChanged;

        public CloudFoundryProvider(PreferencesProvider preferencesProvider)
        {
            this.preferencesProvider = preferencesProvider;
            var preferences          = preferencesProvider.Load();
            this.Clouds              = preferences.Clouds.DeepCopy();            
            this.CloudUrls           = preferences.CloudUrls.DeepCopy();

            this.Clouds.CollectionChanged += Clouds_CollectionChanged;
            foreach (var cloud in Clouds)
            {
                cloud.PropertyChanged += CloudChanged;
            }

            Messenger.Default.Register<NotificationMessageAction<ICloudFoundryProvider>>(this, ProcessCloudFoundryProviderMessage);
        }
        
        private void ProcessCloudFoundryProviderMessage(NotificationMessageAction<ICloudFoundryProvider> message)
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
                case "Url":
                case "TimeoutStart":
                case "TimeoutStop":
                case "ID":
                case "Password":
                case "IsConnected":
                case "IsDisconnected":
                    preferencesProvider.Save(new Preferences() { Clouds = this.Clouds, CloudUrls = this.CloudUrls });
                    break;
                default:
                    break;
            }
        }

        private void Clouds_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.CloudsChanged != null)
            {
                this.CloudsChanged(sender, e);
            }
            SaveChanges();
        }

        public void SaveChanges()
        {
            preferencesProvider.Save(new Preferences() { Clouds = this.Clouds, CloudUrls = this.CloudUrls });
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
                var provisionedServices = client.GetProvisionedServices();
                var availableServices = client.GetSystemServices();                
                local.Applications.Synchronize(new SafeObservableCollection<Application>(applications), new ApplicationEqualityComparer());
                local.Services.Synchronize(new SafeObservableCollection<ProvisionedService>(provisionedServices), new ProvisionedServiceEqualityComparer());
                local.AvailableServices.Synchronize(new SafeObservableCollection<SystemService>(availableServices), new SystemServiceEqualityComparer());
                foreach (Application app in local.Applications)
                {
                    var instances = GetInstances(local, app);
                    if (instances.Response != null)
                        app.InstanceCollection.Synchronize(new SafeObservableCollection<Instance>(instances.Response), new InstanceEqualityComparer());
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
            cloud.Services.Clear();
            cloud.Applications.Clear();
            cloud.AvailableServices.Clear();
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

        public ProviderResponse<IEnumerable<Instance>> GetInstances(Cloud cloud, Application app)
        {
            var response = new ProviderResponse<IEnumerable<Instance>>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                var stats = client.GetStats(app);
                var instances = new SafeObservableCollection<Instance>();
                if (stats != null)
                {

                    foreach (var stat in stats)
                    {
                        var instance = new Instance()
                                       {
                                           ID = stat.ID,
                                           State = stat.State
                                       };
                        if (stat.Stats != null)
                        {
                            instance.Cores = stat.Stats.Cores;
                            instance.MemoryQuota = stat.Stats.MemQuota/1048576;
                            instance.DiskQuota = stat.Stats.DiskQuota/1048576;
                            instance.Host = stat.Stats.Host;
                            instance.Parent = app;
                            instance.Uptime = TimeSpan.FromSeconds(Convert.ToInt32(stat.Stats.Uptime));

                            if (stat.Stats.Usage != null)
                            {
                                instance.Cpu = stat.Stats.Usage.CpuTime/100;
                                instance.Memory = Convert.ToInt32(stat.Stats.Usage.MemoryUsage)/1024;
                                instance.Disk = Convert.ToInt32(stat.Stats.Usage.DiskUsage)/1048576;
                            }
                        }
                        instances.Add(instance);
                    }
                }
                response.Response = instances;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;            
        }

        public ProviderResponse<IEnumerable<StatInfo>> GetStats(Application app, Cloud cloud)
        {
            var response = new ProviderResponse<IEnumerable<StatInfo>>();
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

        public ProviderResponse<Application> GetApplication(Application app, Cloud cloud)
        {
            var response = new ProviderResponse<Application>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                response.Response = client.GetApplication(app.Name);
                var instancesResponse = this.GetInstances(cloud, app);
                if (instancesResponse.Response != null)
                    response.Response.InstanceCollection.Synchronize(new SafeObservableCollection<Instance>(instancesResponse.Response),new InstanceEqualityComparer());
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<SafeObservableCollection<ProvisionedService>> GetProvisionedServices(Cloud cloud)
        {
            var response = new ProviderResponse<SafeObservableCollection<ProvisionedService>>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                response.Response = new SafeObservableCollection<ProvisionedService>(client.GetProvisionedServices());
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<SafeObservableCollection<StatInfo>> GetStats(Cloud cloud, Application application)
        {
            var response = new ProviderResponse<SafeObservableCollection<StatInfo>>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                response.Response = new SafeObservableCollection<StatInfo>(client.GetStats(application));
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public ProviderResponse<bool> UpdateApplication(Application app, Cloud cloud)
        {
            var response = new ProviderResponse<bool>();
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
            var response = new ProviderResponse<bool>();
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
            var response = new ProviderResponse<bool>();
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
            var response = new ProviderResponse<bool>();
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
            var response = new ProviderResponse<bool>();
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

        public ProviderResponse<bool> CreateService(Cloud cloud, string serviceName, string provisionedServiceName)
        {
            var response = new ProviderResponse<bool>();
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

        public ProviderResponse<bool> ChangePassword(Cloud cloud, string newPassword)
        {
            var response = new ProviderResponse<bool>();
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
            var response = new ProviderResponse<bool>();
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

        public ProviderResponse<VcapFilesResult> GetFiles(Cloud cloud, Application application, string path, ushort instanceId)
        {
            var response = new ProviderResponse<VcapFilesResult>();
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
            var response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                var result = client.Push(name, url, instances, new System.IO.DirectoryInfo(directoryToPushFrom), memory, services);
                if (!result.Success)
                    throw new Exception(result.Message);
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
            var response = new ProviderResponse<bool>();
            try
            {
                IVcapClient client = new VcapClient(cloud);
                var result = client.Update(app.Name, new System.IO.DirectoryInfo(directoryToPushFrom));
                if (!result.Success)
                    throw new Exception(result.Message);
                response.Response = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}