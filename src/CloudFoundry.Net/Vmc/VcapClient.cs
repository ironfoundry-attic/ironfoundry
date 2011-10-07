namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Properties;
    using Types;

    public class VcapClient : IVcapClient
    {
        private readonly VcapCredentialManager credMgr;
        private readonly Cloud cloud;

        public VcapClient()
        {
            credMgr = new VcapCredentialManager();
        }

        public VcapClient(string argUri)
        {
            credMgr = new VcapCredentialManager();
            credMgr.SetTarget(argUri);
        }

        public VcapClient(Cloud argCloud)
        {
            credMgr = new VcapCredentialManager();
            credMgr.SetTarget(argCloud.Url);
            cloud = argCloud;
        }

        public string CurrentUri
        {
            get { return credMgr.CurrentTarget.AbsoluteUriTrimmed(); }
        }

        public string CurrentToken
        {
            get { return credMgr.CurrentToken; }
        }

        public VcapClientResult Info()
        {
            var helper = new MiscHelper(credMgr);
            return helper.Info();
        }

        public VcapClientResult Target(string argUri)
        {
            var helper = new MiscHelper(credMgr);

            if (argUri.IsNullOrWhiteSpace())
            {
                return helper.Target();
            }
            else
            {
                return helper.Target(new Uri(argUri));
            }
        }

        public VcapClientResult Login()
        {
            return Login(cloud.Email, cloud.Password);
        }

        public VcapClientResult Login(string argEmail, string argPassword)
        {
            var helper = new UserHelper(credMgr);
            string email = argEmail, password = argPassword;
            return helper.Login(email, password);
        }

        public VcapClientResult Push(
            string argName, string argDeployFQDN, ushort argInstances,
            DirectoryInfo argPath, uint argMemoryMB, string[] argProvisionedServiceNames)
        {
            checkLoginStatus();
            var apps = new AppsHelper(credMgr);
            return apps.Push(argName, argDeployFQDN, argInstances, argPath, argMemoryMB,
                argProvisionedServiceNames, "aspdotnet", "aspdotnet40");
        }

        public void Delete(string argName)
        {
            checkLoginStatus();
            var apps = new AppsHelper(credMgr);
            apps.Delete(argName);
        }

        public VcapClientResult BindService(string argProvisionedServiceName, string argAppName)
        {
            checkLoginStatus();
            var services = new ServicesHelper(credMgr);
            return services.BindService(argProvisionedServiceName, argAppName);
        }

        public VcapClientResult UnbindService(string provisionedServiceName, string appName)
        {
            checkLoginStatus();
            var services = new ServicesHelper(credMgr);
            return services.UnbindService(provisionedServiceName, appName);
        }

        public VcapClientResult CreateService(string argServiceName, string argProvisionedServiceName)
        {
            checkLoginStatus();
            var services = new ServicesHelper(credMgr);
            return services.CreateService(argServiceName, argProvisionedServiceName);
        }

        public VcapClientResult DeleteService(string argProvisionedServiceName)
        {
            checkLoginStatus();
            var services = new ServicesHelper(credMgr);
            return services.DeleteService(argProvisionedServiceName);
        }

        public IEnumerable<SystemService> GetSystemServices()
        {
            checkLoginStatus();
            var services = new ServicesHelper(credMgr);
            return services.GetSystemServices();
        }

        public IEnumerable<ProvisionedService> GetProvisionedServices()
        {
            checkLoginStatus();
            var services = new ServicesHelper(credMgr);
            return services.GetProvisionedServices();
        }

        public void Start(Application argApp)
        {
            checkLoginStatus();
            var apps = new AppsHelper(credMgr);
            apps.Start(argApp);
        }

        public void Stop(Application argApp)
        {
            checkLoginStatus();
            var apps = new AppsHelper(credMgr);
            apps.Stop(argApp);
        }

        public Application GetApplication(string argName)
        {
            checkLoginStatus();
            var app = new AppsHelper(credMgr);
            return app.GetApplication(argName);
        }

        public VcapResponse UpdateApplication(Application argApp)
        {
            checkLoginStatus();
            var app = new AppsHelper(credMgr);
            return app.UpdateApplication(argApp);
        }

        public void Restart(Application argApp)
        {
            checkLoginStatus();
            var app = new AppsHelper(credMgr);
            app.Restart(argApp);
        }

        public string GetLogs(Application argApp, ushort instanceNumber)
        {
            checkLoginStatus();
            var info = new InfoHelper(credMgr);
            return info.GetLogs(argApp, instanceNumber);
        }

        public IEnumerable<StatInfo> GetStats(Application argApp)
        {
            checkLoginStatus();
            var info = new InfoHelper(credMgr);
            return info.GetStats(argApp);
        }

        public IEnumerable<ExternalInstance> GetInstances(Application argApp)
        {
            checkLoginStatus();
            var info = new InfoHelper(credMgr);
            return info.GetInstances(argApp);
        }

        public IEnumerable<Crash> GetAppCrash(Application argApp)
        {
            checkLoginStatus();
            var apps = new AppsHelper(credMgr);
            return apps.GetAppCrash(argApp);
        }

        public IEnumerable<Application> ListApps()
        {
            checkLoginStatus();
            var apps = new AppsHelper(credMgr);
            return apps.ListApps(cloud);
        }

        private void checkLoginStatus()
        {
            VcapClientResult rslt = Info();
            if (false == rslt.Success)
            {
                throw new VmcAuthException(Resources.Vmc_LoginRequired_Message);
            }
        }
    }
}