namespace IronFoundry.Vcap
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using Extensions;
    using Models;

    public class VcapClient : IVcapClient
    {
        private VcapCredentialManager credMgr;
        private readonly Cloud cloud;
        private static readonly Regex FileRe;
        private static readonly Regex DirRe;
        private VcapUser proxyUser;

        static VcapClient()
        {
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            string validFileNameRegexStr = String.Format(@"^([^{0}]+)\s+([0-9]+(?:\.[0-9]+)?[KBMG])$", new String(invalidFileNameChars));
            FileRe = new Regex(validFileNameRegexStr, RegexOptions.Compiled);

            char[] invalidPathChars = Path.GetInvalidPathChars();
            string validPathRegexStr = String.Format(@"^([^{0}]+)/\s+-$", new String(invalidPathChars));
            DirRe = new Regex(validPathRegexStr, RegexOptions.Compiled);
        }

        public VcapClient()
        {
            credMgr = new VcapCredentialManager();
        }

        public VcapClient(string uri)
        {
            Target(uri);
        }

        public VcapClient(Cloud cloud)
        {
            Target(cloud.Url);
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

        public Info GetInfo()
        {
            var helper = new MiscHelper(proxyUser, credMgr);
            return helper.GetInfo();
        }

        internal VcapRequest GetRequestForTesting()
        {
            var helper = new MiscHelper(proxyUser, credMgr);
            return helper.BuildInfoRequest();
        }

        public void Target(string uri)
        {
            Target(uri, null);
        }

        public void Target(string uri, IPAddress ipAddress)
        {
            if (uri.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("uri");
            }

            Uri validatedUri;
            if (!Uri.TryCreate(uri, UriKind.Absolute, out validatedUri))
            {
                validatedUri = new Uri("http://" + uri);
            }

            credMgr = ipAddress == null ? new VcapCredentialManager(validatedUri) : new VcapCredentialManager(validatedUri, ipAddress);
        }

        public string CurrentTarget
        {
            get { return credMgr.CurrentTarget.AbsoluteUriTrimmed(); }
        }

        public void Login()
        {
            Login(cloud.Email, cloud.Password);
        }

        public void Login(string email, string password)
        {
            var helper = new UserHelper(proxyUser, credMgr);
            helper.Login(email, password);
        }

        public void ChangePassword(string newPassword)
        {
            var helper = new UserHelper(proxyUser, credMgr);
            var info = GetInfo();
            helper.ChangePassword(info.User, newPassword);
        }

        public void AddUser(string email, string password)
        {
            var helper = new UserHelper(proxyUser, credMgr);
            helper.AddUser(email, password);
        }

        public void DeleteUser(string email)
        {
            var helper = new UserHelper(proxyUser, credMgr);
            helper.DeleteUser(email);
        }

        public VcapUser GetUser(string email)
        {
            var helper = new UserHelper(proxyUser, credMgr);
            return helper.GetUser(email);
        }

        public IEnumerable<VcapUser> GetUsers()
        {
            var helper = new UserHelper(proxyUser, credMgr);
            return helper.GetUsers();
        }

        public void Push(string name, string deployFQDN, ushort instances,
            DirectoryInfo path, uint memoryMB, string[] provisionedServiceNames)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            helper.Push(name, deployFQDN, instances, path, memoryMB, provisionedServiceNames);
        }

        public void Update(string name, DirectoryInfo path)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            helper.Update(name, path);
        }

        public void BindService(string provisionedServiceName, string appName)
        {
            var services = new ServicesHelper(proxyUser, credMgr);
            services.BindService(provisionedServiceName, appName);
        }

        public void UnbindService(string provisionedServiceName, string appName)
        {
            var services = new ServicesHelper(proxyUser, credMgr);
            services.UnbindService(provisionedServiceName, appName);
        }

        public void CreateService(string serviceName, string provisionedServiceName)
        {
            var services = new ServicesHelper(proxyUser, credMgr);
            services.CreateService(serviceName, provisionedServiceName);
        }

        public void DeleteService(string provisionedServiceName)
        {
            var services = new ServicesHelper(proxyUser, credMgr);
            services.DeleteService(provisionedServiceName);
        }

        public IEnumerable<SystemService> GetSystemServices()
        {
            var services = new ServicesHelper(proxyUser, credMgr);
            return services.GetSystemServices();
        }

        public IEnumerable<ProvisionedService> GetProvisionedServices()
        {
            var services = new ServicesHelper(proxyUser, credMgr);
            return services.GetProvisionedServices();
        }

        public void Start(string appName)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            helper.Start(appName);
        }

        public void Start(Application app)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            helper.Start(app);
        }

        public void Stop(string appName)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            helper.Stop(appName);
        }

        public void Stop(Application app)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            helper.Stop(app);
        }

        public void Restart(string appName)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            helper.Restart(appName);
        }

        public void Restart(Application app)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            helper.Restart(app);
        }

        public void Delete(string appName)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            helper.Delete(appName);
        }

        public void Delete(Application app)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            helper.Delete(app);
        }

        public Application GetApplication(string name)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            Application rv =  helper.GetApplication(name);
            rv.Parent = cloud; // TODO not thrilled about this
            return rv;
        }

        public IEnumerable<Application> GetApplications()
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            IEnumerable<Application> apps = helper.GetApplications();
            foreach (var app in apps) // TODO not thrilled about this
            {
                app.Parent = cloud;
                app.User = proxyUser;
            } 
            return apps;
        }

        public byte[] FilesSimple(string appName, string path, ushort instance)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            return helper.Files(appName, path, instance);
        }

        public VcapFilesResult Files(string appName, string path, ushort instance)
        {
            VcapFilesResult rv;

            var helper = new AppsHelper(proxyUser, credMgr);
            byte[] content = helper.Files(appName, path, instance);
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
                if (FileRe.IsMatch(firstLine) || DirRe.IsMatch(firstLine))
                {
                    // Probably looking at a listing, not a file
                    string contentAscii = Encoding.ASCII.GetString(content);
                    string[] contentAry = contentAscii.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    rv = new VcapFilesResult();
                    foreach (string item in contentAry)
                    {
                        Match fileMatch = FileRe.Match(item);
                        if (fileMatch.Success)
                        {
                            string fileName = fileMatch.Groups[1].Value; // NB: 0 is the entire matched string
                            string fileSize = fileMatch.Groups[2].Value;
                            rv.AddFile(fileName, fileSize);
                            continue;
                        }

                        Match dirMatch = DirRe.Match(item);
                        if (dirMatch.Success)
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

        public void UpdateApplication(Application app)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            helper.UpdateApplication(app);
        }

        public string GetLogs(Application app, ushort instanceNumber)
        {
            var info = new InfoHelper(proxyUser, credMgr);
            return info.GetLogs(app, instanceNumber);
        }

        public IEnumerable<StatInfo> GetStats(Application app)
        {
            var info = new InfoHelper(proxyUser, credMgr);
            return info.GetStats(app);
        }

        public IEnumerable<ExternalInstance> GetInstances(Application app)
        {
            var info = new InfoHelper(proxyUser, credMgr);
            return info.GetInstances(app);
        }

        public IEnumerable<Crash> GetAppCrash(Application app)
        {
            var helper = new AppsHelper(proxyUser, credMgr);
            return helper.GetAppCrash(app);
        }
    }
}