namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.IO;
    using Types;
    using System.Collections.Generic;

    public interface IVcapClient
    {
        VcapClientResult Info();

        VcapClientResult Target(string argUri);

        VcapClientResult Login(Cloud argCloud);
        VcapClientResult Login(string email, string password);

        // TODO VcapClientResult ChangePassword(string username, string newpassword, string url, string accesstoken)

        VcapClientResult Push(string argName, string argDeployFQDN, DirectoryInfo argPath, uint memorysize);
        VcapClientResult Push(Cloud argCloud, string argName, string argDeployFQDN, DirectoryInfo argPath, uint memorysize);

        void Stop(Cloud argCloud, Application argApplication);

        void Start(Cloud argCloud, Application argApplication);

        Application GetAppInfo(Cloud argCloud, string argName);

        void RestartApp(Application application, Cloud cloud);

        string GetLogs(Application application, int instanceNumber, Cloud cloud);

        // SortedDictionary<int, StatInfo> GetStats(Application application, Cloud cloud);
        IEnumerable<StatInfo> GetStats(Application application, Cloud cloud);

        IEnumerable<ExternalInstance> GetInstances(Application application, Cloud cloud);

        IEnumerable<Crash> GetAppCrash(Application application, Cloud cloud);

        IEnumerable<Application> ListApps(Cloud cloud);

        IEnumerable<SystemServices> GetAvailableServices(Cloud cloud);

        IEnumerable<AppService> GetProvisionedServices(Cloud cloud);

        VcapResponse UpdateApplicationSettings(Application application, Cloud cloud);
    }
}