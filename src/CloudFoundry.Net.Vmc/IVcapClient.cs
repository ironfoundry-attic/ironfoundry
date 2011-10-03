namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using CloudFoundry.Net.Types;

    public interface IVcapClient
    {
        VcapClientResult Info();

        VcapClientResult Target(string argUri);

        VcapClientResult Login(Cloud argCloud);
        VcapClientResult Login(string email, string password);

        // TODO VcapClientResult ChangePassword(string username, string newpassword, string url, string accesstoken)

        string Push(string appname, string fdqn, string fileURI, string framework, string memorysize);

        void StopApp(Application application, Cloud cloud);

        void StartApp(Application application, Cloud cloud);

        Application GetAppInfo(String appname, Cloud cloud);

        void RestartApp(Application application, Cloud cloud);

        string GetLogs(Application application, int instanceNumber, Cloud cloud);

        SortedDictionary<int,StatInfo> GetStats(Application application, Cloud cloud);

        List<ExternalInstance> GetInstances(Application application, Cloud cloud);

        List<Crash> GetAppCrash(Application application, Cloud cloud);

        List<Application> ListApps(Cloud cloud);

        List<SystemServices> GetAvailableServices(Cloud cloud);

        List<AppService> GetProvisionedServices(Cloud cloud);

        VcapResponse UpdateApplicationSettings(Application application, Cloud cloud);
    }
}