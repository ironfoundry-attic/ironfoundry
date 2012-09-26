namespace IronFoundry.Vcap
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using Models;

    public interface IVcapClient
    {
        string CurrentToken { get; }
        string CurrentUri { get; }

        void ProxyAs(VcapUser user);

        Info GetInfo();

        void Target(string uri);
        void Target(string uri, IPAddress ipAddress);
        string CurrentTarget { get; }

        void Login();
        void Login(string email, string password);
        void ChangePassword(string newPassword);
        void AddUser(string email, string password);
        void DeleteUser(string email);
        VcapUser GetUser(string email);
        IEnumerable<VcapUser> GetUsers();

        void Push(string name, string deployFQDN, ushort instances, DirectoryInfo path,
            uint memoryMB, string[] provisionedServiceNames);

        void Update(string appname, DirectoryInfo di);

        void BindService(string appName, string provisionedServiceName);
        void CreateService(string serviceName, string provisionedServiceName);
        void DeleteService(string provisionedServiceName);
        void UnbindService(string provisionedServiceName, string appName);

        IEnumerable<SystemService> GetSystemServices();
        IEnumerable<ProvisionedService> GetProvisionedServices();

        void Stop(Application app);
        void Stop(string appName);

        void Start(Application app);
        void Start(string appName);

        void Restart(Application app);
        void Restart(string appName);

        void Delete(Application app);
        void Delete(string appName);

        Application GetApplication(string appName);
        IEnumerable<Application> GetApplications();
        byte[] FilesSimple(string appName, string path, ushort instance);
        VcapFilesResult Files(string appName, string path, ushort instance);

        string GetLogs(Application application, ushort instanceNumber);

        IEnumerable<StatInfo> GetStats(Application application);

        IEnumerable<ExternalInstance> GetInstances(Application application);

        IEnumerable<Crash> GetAppCrash(Application application);

        void UpdateApplication(Application application);
    }
}