namespace CloudFoundry.Net.Vmc
{
    using System.Collections.Generic;
    using System.IO;
    using Types;

    public interface IVcapClient
    {
        /*
         * TODO: better way to indicate success for methods that don't return VcapClientResult? hm.
         */
        string CurrentToken { get; }
        string CurrentUri { get; }

        VcapClientResult Info();

        VcapClientResult Target(string uri);

        VcapClientResult Login();
        VcapClientResult Login(string email, string password);
        VcapClientResult ChangePassword(string newpassword);
        VcapClientResult AddUser(string email, string password);

        VcapClientResult Push(
            string name, string deployFQDN, ushort instances, DirectoryInfo path,
            uint memoryKB, string[] provisionedServiceNames);

        VcapClientResult Update(string appname, DirectoryInfo di);

        VcapClientResult BindService(string appName, string provisionedServiceName);
        VcapClientResult CreateService(string serviceName, string provisionedServiceName);
        VcapClientResult DeleteService(string provisionedServiceName);
        VcapClientResult UnbindService(string provisionedServiceName, string appName);

        IEnumerable<SystemService> GetSystemServices();
        IEnumerable<ProvisionedService> GetProvisionedServices();

        void Stop(Application app);
        void Start(Application app);
        void Restart(Application app);
        void Delete(string appName);

        Application GetApplication(string name);
        IEnumerable<Application> GetApplications();
        string Files(string name, string path, ushort instance);

        string GetLogs(Application application, ushort instanceNumber);

        IEnumerable<StatInfo> GetStats(Application application);

        IEnumerable<ExternalInstance> GetInstances(Application application);

        IEnumerable<Crash> GetAppCrash(Application application);

        VcapResponse UpdateApplication(Application application);
    }
}