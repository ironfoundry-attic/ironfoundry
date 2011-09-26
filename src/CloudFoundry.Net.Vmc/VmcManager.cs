using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.Vmc
{
    public class VmcManager : IVmcClient
    {
        public string URL { get; set; }
        public string AccessToken { get; set; }

        public string LogIn(string email, string password)
        {
            VmcAdministration cfa = new VmcAdministration();
            return cfa.Login(email, password, URL);
        }

        public Cloud LogIn(Cloud cloud){
            VmcAdministration cfa = new VmcAdministration();
            return cfa.Login(cloud);
        }
        
        public string Push(string appname, string fdqn, string fileURI, string framework, string memorysize)
        {
            VmcApps cfapps = new VmcApps();
            var app =  cfapps.PushApp(appname, URL, AccessToken, fileURI, fdqn, framework, null,memorysize, null);
            return app;
        }

        public string Info()
        {
            VmcInit init = new VmcInit();
            return init.Info(AccessToken,URL);
        }


        public void StopApp(Application application, Cloud cloud)
        {
            VmcApps apps = new VmcApps();
            apps.StopApp(application, cloud);
        }

        public void StartApp(Application application, Cloud cloud)
        {
            VmcApps apps = new VmcApps();
            apps.StartApp(application, cloud);
        }

        public Application GetAppInfo(string appname, Cloud cloud)
        {
            VmcApps app = new VmcApps();
            return app.GetAppInfo(appname, cloud);
        }


        public void RestartApp(Application application, Cloud cloud)
        {
            VmcApps app = new VmcApps();
            app.RestartApp(application, cloud);
        }


        public string GetLogs(Application application, int instanceNumber, Cloud cloud)
        {
            VmcInfo info = new VmcInfo();
            return info.GetLogs(application, instanceNumber, cloud);
        }

        public List<Stats> GetStats(Application application, Cloud cloud)
        {
            VmcInfo info = new VmcInfo();
            return info.GetStats(application, cloud);
        }

        public List<Instance> GetInstances(Application application, Cloud cloud)
        {
            VmcInfo info = new VmcInfo();
            return info.GetInstances(application, cloud);
        }

        public List<Crash> GetAppCrash(Application application, Cloud cloud)
        {
            VmcApps apps = new VmcApps();
            return apps.GetAppCrash(application, cloud);
        }

        public List<Application> ListApps(Cloud cloud)
        {
            VmcApps apps = new VmcApps();
            return apps.ListApps(cloud);
        }
    }
}
