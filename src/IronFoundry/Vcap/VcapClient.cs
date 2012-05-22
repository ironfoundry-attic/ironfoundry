namespace IronFoundry.Vcap
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using IronFoundry.Properties;
    using IronFoundry.Types;

    public class VcapClient : IVcapClient
    {
        private readonly VcapCredentialManager credMgr;
        private readonly Cloud cloud;
        private Info info;
        private static readonly Regex file_re;
        private static readonly Regex dir_re;
        private VcapUser proxyUser;

        static VcapClient()
        {
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            string validFileNameRegexStr = String.Format(@"^([^{0}]+)\s+([0-9]+(?:\.[0-9]+)?[KBMG])$", new String(invalidFileNameChars));
            file_re = new Regex(validFileNameRegexStr, RegexOptions.Compiled);

            char[] invalidPathChars = Path.GetInvalidPathChars();
            string validPathRegexStr = String.Format(@"^([^{0}]+)/\s+-$", new String(invalidPathChars));
            dir_re = new Regex(validPathRegexStr, RegexOptions.Compiled);
        }

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

        public VcapClient(Uri uri, IPAddress ipAddress)
        {
            credMgr = new VcapCredentialManager(uri, ipAddress);
        }

        public void ProxyAs(VcapUser user)
        {
            proxyUser = user;
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
            var helper = new MiscHelper(proxyUser, credMgr);
            return helper.Info();
        }

        internal VcapRequest GetRequestForTesting()
        {
            var helper = new MiscHelper(proxyUser, credMgr);
            return helper.BuildInfoRequest();
        }

        public VcapClientResult Target(string uri)
        {
            var helper = new MiscHelper(proxyUser, credMgr);

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
            var helper = new UserHelper(proxyUser, credMgr);
            return helper.Login(email, password);
        }

        public VcapClientResult ChangePassword(string newpassword)
        {
            CheckLoginStatus();
            var hlpr = new UserHelper(proxyUser, credMgr);
            return hlpr.ChangePassword(info.User, newpassword);
        }

        public VcapClientResult AddUser(string email, string password)
        {
            var hlpr = new UserHelper(proxyUser, credMgr);
            return hlpr.AddUser(email, password);
        }

        public VcapClientResult DeleteUser(string email)
        {
            var hlpr = new UserHelper(proxyUser, credMgr);
            return hlpr.DeleteUser(email);
        }

        public VcapUser GetUser(string email)
        {
            var hlpr = new UserHelper(proxyUser, credMgr);
            return hlpr.GetUser(email);
        }

        public IEnumerable<VcapUser> GetUsers()
        {
            var hlpr = new UserHelper(proxyUser, credMgr);
            return hlpr.GetUsers();
        }

        public VcapClientResult Push(
            string name, string deployFQDN, ushort instances,
            DirectoryInfo path, uint memoryMB, string[] provisionedServiceNames)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.Push(name, deployFQDN, instances, path, memoryMB, provisionedServiceNames);
        }

        public VcapClientResult Update(string name, DirectoryInfo path)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.Update(name, path);
        }

        public VcapClientResult BindService(string provisionedServiceName, string appName)
        {
            CheckLoginStatus();
            var services = new ServicesHelper(proxyUser, credMgr);
            return services.BindService(provisionedServiceName, appName);
        }

        public VcapClientResult UnbindService(string provisionedServiceName, string appName)
        {
            CheckLoginStatus();
            var services = new ServicesHelper(proxyUser, credMgr);
            return services.UnbindService(provisionedServiceName, appName);
        }

        public VcapClientResult CreateService(string serviceName, string provisionedServiceName)
        {
            CheckLoginStatus();
            var services = new ServicesHelper(proxyUser, credMgr);
            return services.CreateService(serviceName, provisionedServiceName);
        }

        public VcapClientResult DeleteService(string provisionedServiceName)
        {
            CheckLoginStatus();
            var services = new ServicesHelper(proxyUser, credMgr);
            return services.DeleteService(provisionedServiceName);
        }

        public IEnumerable<SystemService> GetSystemServices()
        {
            CheckLoginStatus();
            var services = new ServicesHelper(proxyUser, credMgr);
            return services.GetSystemServices();
        }

        public IEnumerable<ProvisionedService> GetProvisionedServices()
        {
            CheckLoginStatus();
            var services = new ServicesHelper(proxyUser, credMgr);
            return services.GetProvisionedServices();
        }

        public VcapClientResult Start(string appName)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.Start(appName);
        }

        public VcapClientResult Start(Application app)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.Start(app);
        }

        public VcapClientResult Stop(string appName)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.Stop(appName);
        }

        public VcapClientResult Stop(Application app)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.Stop(app);
        }

        public VcapClientResult Restart(string appName)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.Restart(appName);
        }

        public VcapClientResult Restart(Application app)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.Restart(app);
        }

        public VcapClientResult Delete(string appName)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.Delete(appName);
        }

        public VcapClientResult Delete(Application app)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.Delete(app);
        }

        public Application GetApplication(string name)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            Application rv =  hlpr.GetApplication(name);
            rv.Parent = cloud; // TODO not thrilled about this
            return rv;
        }

        public IEnumerable<Application> GetApplications()
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            IEnumerable<Application> apps = hlpr.GetApplications();
            foreach (var app in apps) // TODO not thrilled about this
            {
                app.Parent = cloud;
                app.User = proxyUser;
            } 
            return apps;
        }

        public byte[] FilesSimple(string appName, string path, ushort instance)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.Files(appName, path, instance);
        }

        public VcapFilesResult Files(string appName, string path, ushort instance)
        {
            CheckLoginStatus();

            VcapFilesResult rv;

            var hlpr = new AppsHelper(proxyUser, credMgr);
            byte[] content = hlpr.Files(appName, path, instance);
            if (null == content)
            {
                rv = new VcapFilesResult(false);
            }
            else if (content.Length == 0)
            {
                rv = new VcapFilesResult(content);
            }
            else
            {
                int i = 0;
                for (i = 0; i < content.Length; ++i)
                {
                    if (content[i] == '\n')
                    {
                        break;
                    }
                }
                string firstLine = Encoding.ASCII.GetString(content, 0, i);
                if (file_re.IsMatch(firstLine) || dir_re.IsMatch(firstLine))
                {
                    // Probably looking at a listing, not a file
                    string contentAscii = Encoding.ASCII.GetString(content);
                    string[] contentAry = contentAscii.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    rv = new VcapFilesResult();
                    foreach (string item in contentAry)
                    {
                        Match fileMatch = file_re.Match(item);
                        if (null != fileMatch && fileMatch.Success)
                        {
                            string fileName = fileMatch.Groups[1].Value; // NB: 0 is the entire matched string
                            string fileSize = fileMatch.Groups[2].Value;
                            rv.AddFile(fileName, fileSize);
                            continue;
                        }

                        Match dirMatch = dir_re.Match(item);
                        if (null != dirMatch && dirMatch.Success)
                        {
                            string dirName = dirMatch.Groups[1].Value;
                            rv.AddDirectory(dirName);
                            continue;
                        }

                        throw new InvalidOperationException("Match failed.");
                    }
                }
                else
                {
                    rv = new VcapFilesResult(content);
                }
            }

            return rv;
        }

        public VcapResponse UpdateApplication(Application app)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.UpdateApplication(app);
        }

        public string GetLogs(Application app, ushort instanceNumber)
        {
            CheckLoginStatus();
            var info = new InfoHelper(proxyUser, credMgr);
            return info.GetLogs(app, instanceNumber);
        }

        public IEnumerable<StatInfo> GetStats(Application app)
        {
            CheckLoginStatus();
            var info = new InfoHelper(proxyUser, credMgr);
            return info.GetStats(app);
        }

        public IEnumerable<ExternalInstance> GetInstances(Application app)
        {
            CheckLoginStatus();
            var info = new InfoHelper(proxyUser, credMgr);
            return info.GetInstances(app);
        }

        public IEnumerable<Crash> GetAppCrash(Application app)
        {
            CheckLoginStatus();
            var hlpr = new AppsHelper(proxyUser, credMgr);
            return hlpr.GetAppCrash(app);
        }

        private void CheckLoginStatus()
        {
            if (null == info)
            {
                if (false == LoggedIn())
                {
                    throw new VmcAuthException(Resources.Vmc_LoginRequired_Message);
                }
            }
        }

        private bool LoggedIn()
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