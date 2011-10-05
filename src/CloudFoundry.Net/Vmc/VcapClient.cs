namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Properties;
    using Types;

    public class VcapClient : IVcapClient
    {
        private readonly VcapCredentialManager credentialManager;
        private readonly Cloud cloud;

        public VcapClient()
        {
            credentialManager = new VcapCredentialManager();
        }

        public VcapClient(string argUri)
        {
            credentialManager = new VcapCredentialManager();
            credentialManager.SetTarget(argUri);
        }

        public VcapClient(Cloud argCloud)
        {
            credentialManager = new VcapCredentialManager();
            credentialManager.SetTarget(argCloud.Url);
            cloud = argCloud;
        }

        public string CurrentUri
        {
            get { return credentialManager.CurrentTarget.AbsoluteUriTrimmed(); }
        }

        public Uri CurrentTarget
        {
            get { return credentialManager.CurrentTarget; }
        }

        public string CurrentToken
        {
            get { return credentialManager.CurrentToken; }
        }

        public VcapClientResult Info()
        {
            var helper = new MiscHelper(credentialManager);
            return helper.Info();
        }

        public VcapClientResult Target(string argUri)
        {
            var helper = new MiscHelper(credentialManager);

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
            var helper = new UserHelper(credentialManager);
            string email = argEmail, password = argPassword;
            return helper.Login(email, password);
        }

        public VcapClientResult Push(string argName, string argDeployFQDN, DirectoryInfo argPath, uint argMemory)
        {
            checkLoginStatus();
            var apps = new AppsHelper(credentialManager);
            return apps.Push(argName, argPath, argDeployFQDN, "aspdotnet", "aspdotnet40", argMemory, null);
        }

        public VcapClientResult Delete(string argName)
        {
            checkLoginStatus();
            var apps = new AppsHelper(credentialManager);
            apps.Delete(argName);
            return new VcapClientResult();
        }

        public VcapClientResult Bind(string argProvisionedServiceName, string argAppName)
        {
            checkLoginStatus();
            var services = new ServicesHelper(credentialManager);
            return services.BindService(argProvisionedServiceName, argAppName);
        }

        public IEnumerable<SystemService> GetSystemServices()
        {
            checkLoginStatus();
            var services = new ServicesHelper(credentialManager);
            return services.GetSystemServices();
        }

        public IEnumerable<ProvisionedService> GetProvisionedServices()
        {
            checkLoginStatus();
            var services = new ServicesHelper(credentialManager);
            return services.GetProvisionedServices();
        }

        public void Start(Application argApp)
        {
            var apps = new AppsHelper(credentialManager);
            apps.Start(argApp);
        }

        public void Stop(Application argApp)
        {
            var apps = new AppsHelper(credentialManager);
            apps.Stop(argApp);
        }

        public Application GetAppInfo(string argName)
        {
            var app = new AppsHelper(credentialManager);
            return app.GetApplication(argName);
        }

        public VcapResponse UpdateApplication(Application argApp)
        {
            var app = new AppsHelper(credentialManager);
            return app.UpdateApplication(argApp);
        }

        public void Restart(Application argApp)
        {
            var app = new AppsHelper(credentialManager);
            app.Restart(argApp);
        }

        public string GetLogs(Application argApp, ushort instanceNumber)
        {
            var info = new InfoHelper(credentialManager);
            return info.GetLogs(argApp, instanceNumber);
        }

        public IEnumerable<StatInfo> GetStats(Application argApp)
        {
            var info = new InfoHelper(credentialManager);
            return info.GetStats(argApp);
        }

        public IEnumerable<ExternalInstance> GetInstances(Application argApp)
        {
            var info = new InfoHelper(credentialManager);
            return info.GetInstances(argApp);
        }

        public IEnumerable<Crash> GetAppCrash(Application argApp)
        {
            var apps = new AppsHelper(credentialManager);
            return apps.GetAppCrash(argApp);
        }

        public IEnumerable<Application> ListApps()
        {
            var apps = new AppsHelper(credentialManager);
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