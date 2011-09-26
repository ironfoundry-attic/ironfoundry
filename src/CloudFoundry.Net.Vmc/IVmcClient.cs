using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.Vmc
{
    public interface IVmcClient
    {
        string URL { get; set; }
        string AccessToken { get; set; }
        string LogIn (string email, string password);
        string Push(string appname, string fdqn, string fileURI, string framework, string memorysize);
        void StopApp(Application application, Cloud cloud);
        void StartApp(Application application, Cloud cloud);
        Application GetAppInfo(String appname, Cloud cloud);
        void RestartApp(Application application, Cloud cloud);
        string GetLogs(Application application, int instanceNumber, Cloud cloud);
        List<Stats> GetStats(Application application, Cloud cloud);
        List<Instance> GetInstances(Application application, Cloud cloud);
        List<Crash> GetAppCrash(Application application, Cloud cloud);
        List<Application> ListApps(Cloud cloud);
        string Info();
    }
}
