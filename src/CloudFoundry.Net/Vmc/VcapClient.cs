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
        private Info info;

        public VcapClient()
        {
            credMgr = new VcapCredentialManager();
        }

        public VcapClient(string uri)
        {
            credMgr = new VcapCredentialManager();
            credMgr.SetTarget(uri);
        }

        public VcapClient(Cloud cloud)
        {
            credMgr = new VcapCredentialManager();
            credMgr.SetTarget(cloud.Url);
            this.cloud = cloud;
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

        public VcapClientResult Target(string uri)
        {
            var helper = new MiscHelper(credMgr);

            if (uri.IsNullOrWhiteSpace())
            {
                return helper.Target();
            }
            else
            {
                Uri tmp;
                if (Uri.TryCreate(uri, UriKind.Absolute, out tmp))
                {
                    return helper.Target(tmp);
                }
                else
                {
                    return helper.Target(new Uri("http://" + uri));
                }
            }
        }

        public VcapClientResult Login()
        {
            return Login(cloud.Email, cloud.Password);
        }

        public VcapClientResult Login(string email, string password)
        {
            var helper = new UserHelper(credMgr);
            return helper.Login(email, password);
        }

        public VcapClientResult ChangePassword(string newpassword)
        {
            checkLoginStatus();
            var hlpr = new UserHelper(credMgr);
            return hlpr.ChangePassword(info.User, newpassword);
        }

        public VcapClientResult AddUser(string email, string password)
        {
            var hlpr = new UserHelper(credMgr);
            return hlpr.AddUser(email, password);
        }

        public VcapClientResult Push(
            string name, string deployFQDN, ushort instances,
            DirectoryInfo path, uint memoryMB, string[] provisionedServiceNames)
        {
            checkLoginStatus();
            var hlpr = new AppsHelper(credMgr);
            return hlpr.Push(name, deployFQDN, instances, path, memoryMB,
                provisionedServiceNames, "aspdotnet", "aspdotnet40");
        }

        public VcapClientResult Update(string name, DirectoryInfo path)
        {
            checkLoginStatus();
            var hlpr = new AppsHelper(credMgr);
            return hlpr.Update(name, path);
        }

        public void Delete(string name)
        {
            checkLoginStatus();
            var hlpr = new AppsHelper(credMgr);
            hlpr.Delete(name);
        }

        public VcapClientResult BindService(string provisionedServiceName, string appName)
        {
            checkLoginStatus();
            var services = new ServicesHelper(credMgr);
            return services.BindService(provisionedServiceName, appName);
        }

        public VcapClientResult UnbindService(string provisionedServiceName, string appName)
        {
            checkLoginStatus();
            var services = new ServicesHelper(credMgr);
            return services.UnbindService(provisionedServiceName, appName);
        }

        public VcapClientResult CreateService(string serviceName, string provisionedServiceName)
        {
            checkLoginStatus();
            var services = new ServicesHelper(credMgr);
            return services.CreateService(serviceName, provisionedServiceName);
        }

        public VcapClientResult DeleteService(string provisionedServiceName)
        {
            checkLoginStatus();
            var services = new ServicesHelper(credMgr);
            return services.DeleteService(provisionedServiceName);
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

        public void Start(Application app)
        {
            checkLoginStatus();
            var hlpr = new AppsHelper(credMgr);
            hlpr.Start(app);
        }

        public void Stop(Application app)
        {
            checkLoginStatus();
            var hlpr = new AppsHelper(credMgr);
            hlpr.Stop(app);
        }

        public Application GetApplication(string name)
        {
            checkLoginStatus();
            var hlpr = new AppsHelper(credMgr);
            Application rv =  hlpr.GetApplication(name);
            rv.Parent = cloud; // TODO not thrilled about this
            return rv;
        }

        public IEnumerable<Application> GetApplications()
        {
            checkLoginStatus();
            var hlpr = new AppsHelper(credMgr);
            IEnumerable<Application> apps = hlpr.GetApplications();
            foreach (var a in apps) { a.Parent = cloud; } // TODO not thrilled about this
            return apps;
        }

        public string Files(string name, string path, ushort instance)
        {
            checkLoginStatus();
            var hlpr = new AppsHelper(credMgr);
            return hlpr.Files(name, path, instance);
        }

        public VcapResponse UpdateApplication(Application app)
        {
            checkLoginStatus();
            var hlpr = new AppsHelper(credMgr);
            return hlpr.UpdateApplication(app);
        }

        public void Restart(Application app)
        {
            checkLoginStatus();
            var hlpr = new AppsHelper(credMgr);
            hlpr.Restart(app);
        }

        public string GetLogs(Application app, ushort instanceNumber)
        {
            checkLoginStatus();
            var info = new InfoHelper(credMgr);
            return info.GetLogs(app, instanceNumber);
        }

        public IEnumerable<StatInfo> GetStats(Application app)
        {
            checkLoginStatus();
            var info = new InfoHelper(credMgr);
            return info.GetStats(app);
        }

        public IEnumerable<ExternalInstance> GetInstances(Application app)
        {
            checkLoginStatus();
            var info = new InfoHelper(credMgr);
            return info.GetInstances(app);
        }

        public IEnumerable<Crash> GetAppCrash(Application app)
        {
            checkLoginStatus();
            var hlpr = new AppsHelper(credMgr);
            return hlpr.GetAppCrash(app);
        }

        private void checkLoginStatus()
        {
            if (null == info)
            {
                if (false == loggedIn())
                {
                    throw new VmcAuthException(Resources.Vmc_LoginRequired_Message);
                }
            }
        }

        private bool loggedIn()
        {
            bool rv = false;

            VcapClientResult rslt = Info();
            if (rslt.Success)
            {
                info = rslt.GetResponseMessage<Info>();
                rv = true;
            }

            return rv;
        }
    }
}