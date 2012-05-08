namespace IronFoundry.Vcap
{
    using System.Collections.Generic;
    using System.IO;
    using IronFoundry.Types;

    public interface IVcapClient
    {
        string CurrentToken { get; }
        string CurrentUri { get; }

        void ProxyAs(VcapUser user);

        VcapClientResult Info();

        VcapClientResult Target(string uri);

        VcapClientResult Login();
        VcapClientResult Login(string email, string password);
        VcapClientResult ChangePassword(string newpassword);
        VcapClientResult AddUser(string email, string password);
        VcapClientResult DeleteUser(string email);
        VcapUser GetUser(string email);
        IEnumerable<VcapUser> GetUsers();

        VcapClientResult Push(
            string name, string deployFQDN, ushort instances, DirectoryInfo path,
            uint memoryMB, string[] provisionedServiceNames);

        VcapClientResult Update(string appname, DirectoryInfo di);

        VcapClientResult BindService(string appName, string provisionedServiceName);
        VcapClientResult CreateService(string serviceName, string provisionedServiceName);
        VcapClientResult DeleteService(string provisionedServiceName);
        VcapClientResult UnbindService(string provisionedServiceName, string appName);

        IEnumerable<SystemService> GetSystemServices();
        IEnumerable<ProvisionedService> GetProvisionedServices();

        VcapClientResult Stop(Application app);
        VcapClientResult Stop(string appName);

        VcapClientResult Start(Application app);
        VcapClientResult Start(string appName);

        VcapClientResult Restart(Application app);
        VcapClientResult Restart(string appName);

        VcapClientResult Delete(Application app);
        VcapClientResult Delete(string appName);

        Application GetApplication(string appName);
        IEnumerable<Application> GetApplications();
        byte[] FilesSimple(string appName, string path, ushort instance);
        VcapFilesResult Files(string appName, string path, ushort instance);

        string GetLogs(Application application, ushort instanceNumber);

        IEnumerable<StatInfo> GetStats(Application application);

        IEnumerable<ExternalInstance> GetInstances(Application application);

        IEnumerable<Crash> GetAppCrash(Application application);

        VcapResponse UpdateApplication(Application application);
    }
}