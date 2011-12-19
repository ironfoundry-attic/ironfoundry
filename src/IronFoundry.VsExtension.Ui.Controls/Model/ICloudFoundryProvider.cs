

namespace IronFoundry.VsExtension.Ui.Controls.Model
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using IronFoundry.Types;
    using IronFoundry.Vcap;
    using IronFoundry.Extensions;

    public interface ICloudFoundryProvider
    {
        SafeObservableCollection<Cloud> Clouds { get; }
        SafeObservableCollection<CloudUrl> CloudUrls { get; }
        event NotifyCollectionChangedEventHandler CloudsChanged;
        void SaveChanges();
        ProviderResponse<Cloud> Connect(Cloud cloud);        
        Cloud Disconnect(Cloud cloud);
        ProviderResponse<IEnumerable<Instance>> GetInstances(Cloud cloud, Application app);
        ProviderResponse<bool> ValidateAccount(Cloud cloud);
        ProviderResponse<IEnumerable<StatInfo>> GetStats(Application app, Cloud cloud);
        ProviderResponse<bool> UpdateApplication(Application app, Cloud cloud);
        ProviderResponse<bool> Start(Application app, Cloud cloud);
        ProviderResponse<bool> Stop(Application app, Cloud cloud);
        ProviderResponse<bool> Restart(Application app, Cloud cloud);
        ProviderResponse<bool> Delete(Application app, Cloud cloud);
        ProviderResponse<Application> GetApplication(Application app, Cloud cloud);
        ProviderResponse<bool> CreateService(Cloud cloud, string serviceName, string provisionedServiceName);
        ProviderResponse<SafeObservableCollection<ProvisionedService>> GetProvisionedServices(Cloud cloud);
        ProviderResponse<bool> ChangePassword(Cloud cloud, string newPassword);
        ProviderResponse<bool> RegisterAccount(Cloud cloud,string email, string password);
        ProviderResponse<SafeObservableCollection<StatInfo>> GetStats(Cloud cloud, Application application);
        ProviderResponse<VcapFilesResult> GetFiles(Cloud cloud, Application application, string path, ushort instanceId);
        ProviderResponse<bool> Push(Cloud cloud, string name, string url, ushort instances, string directoryToPushFrom, uint memory, string[] services);
        ProviderResponse<bool> Update(Cloud cloud, Application app, string directoryToPushFrom);
    }
}