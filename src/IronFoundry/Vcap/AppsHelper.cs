namespace IronFoundry.Vcap
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading;
    using ICSharpCode.SharpZipLib.Zip;
    using IronFoundry;
    using Models;
    using Newtonsoft.Json;
    using Properties;
    using RestSharp;

    internal class AppsHelper : BaseVmcHelper
    {
        const ushort timeoutSeconds = 180;

        public AppsHelper(VcapUser proxyUser, VcapCredentialManager credentialManager)
            : base(proxyUser, credentialManager) { }

        public void Start(string applicationName)
        {
            var application = GetApplication(applicationName);
            Start(application);
        }

        public void Start(Application application)
        {
            application.Start();
            UpdateApplication(application);
            if (!IsStarted(application.Name, 180))
            {
                throw new VcapException("Failed to start application.");
            }
        }

        public void Stop(string applicationName)
        {
            var application = GetApplication(applicationName);
            Stop(application);
        }

        public void Stop(Application application)
        {
            application.Stop();
            UpdateApplication(application);
        }

        public void Restart(string applicationName)
        {
            Stop(applicationName);
            Start(applicationName);
        }

        public void Restart(Application applicationName)
        {
            Stop(applicationName);
            Start(applicationName);
        }

        public void Delete(string applicationName)
        {
            var application = GetApplication(applicationName);
            Delete(application);
        }

        public void Delete(Application application)
        {
            var r = BuildVcapJsonRequest(Method.DELETE, Constants.APPS_PATH, application.Name);
            r.Execute();
        }

        public void UpdateApplication(Application application)
        {
            var r = BuildVcapJsonRequest(Method.PUT, Constants.APPS_PATH, application.Name);
            r.AddBody(application);
            var response = r.Execute<VcapResponse>();

            if (response != null && !string.IsNullOrEmpty(response.Description))
            {
                throw new VcapException(response.Description);
            }
        }

        public byte[] Files(string name, string path, ushort instance)
        {
            var r = base.BuildVcapRequest(Constants.APPS_PATH, name, "instances", instance, "files", path);
            IRestResponse response = r.Execute();
            return response.RawBytes;
        }

        public void Push(string name, string deployFQDN, ushort instances,
            DirectoryInfo path, uint memoryMB, string[] provisionedServiceNames)
        {
            if (path == null)
            {
                throw new ArgumentException("Application local location is needed");
            }

            if (deployFQDN == null)
            {
                throw new ArgumentException("Please specify the url to deploy as.");
            }

            DetetectedFramework framework = FrameworkDetetctor.Detect(path);
            if (framework == null)
            {
                throw new InvalidOperationException("Please specify application framework");
            }
            else
            {
                if (AppExists(name))
                {
                    throw new VcapException(String.Format(Resources.AppsHelper_PushApplicationExists_Fmt, name));
                }
                else
                {
                    /*
                     * Before creating the app, ensure we can build resource list
                     */
                    var resources = new List<Resource>();
                    AddDirectoryToResources(resources, path, path.FullName);

                    var manifest = new AppManifest
                    {
                        Name = name,
                        Staging = new Staging { Framework = framework.Framework, Runtime = framework.Runtime },
                        Uris = new [] { deployFQDN },
                        Instances = instances,
                        Resources = new AppResources { Memory = memoryMB },
                    };

                    var r = BuildVcapJsonRequest(Method.POST, Constants.APPS_PATH);
                    r.AddBody(manifest);
                    r.Execute();

                    UploadAppBits(name, path);

                    Application app = GetApplication(name);
                    app.Start();
                    r = BuildVcapJsonRequest(Method.PUT, Constants.APPS_PATH, name);
                    r.AddBody(app);
                    r.Execute();

                    bool started = IsStarted(app.Name, timeoutSeconds);

                    if (started && !provisionedServiceNames.IsNullOrEmpty())
                    {
                        foreach (string serviceName in provisionedServiceNames)
                        {
                            var servicesHelper = new ServicesHelper(ProxyUser, CredentialManager);
                            servicesHelper.BindService(serviceName, app.Name);
                        }
                    }
                }
            }
        }

        public void Update(string name, DirectoryInfo path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            else
            {
                UploadAppBits(name, path);
                Application app = GetApplication(name);
                Restart(app);
            }
        }

        public string GetAppCrash(string name)
        {
            var r = base.BuildVcapRequest(Constants.APPS_PATH, name, "crashes");
            return r.Execute().Content;
        }

        public IEnumerable<Crash> GetAppCrash(Application app)
        {
            var r = base.BuildVcapRequest(Constants.APPS_PATH, app.Name, "crashes");
            return r.Execute<Crash[]>();
        }

        public IEnumerable<Application> GetApplications(VcapUser user)
        {
            return base.GetApplications(user.Email);
        }

        private bool IsStarted(string name, ushort timeoutSeconds)
        {
            const int sleepSeconds = 3;
            var sleepSpan = TimeSpan.FromSeconds(sleepSeconds);

            bool started = false;

            int tries = timeoutSeconds / sleepSeconds;
            for (int i = 0; i < tries; ++i)
            {
                Application app = GetApplication(name);

                if (app.IsStarted &&
                    (app.RunningInstances.HasValue && app.RunningInstances.Value > 0))
                {
                    started = true;
                    break;
                }
                else
                {
                    Thread.Sleep(sleepSpan);
                }
            }

            return started;
        }

        private void UploadAppBits(string name, DirectoryInfo path)
        {
            /*
             * Before creating the app, ensure we can build resource list
             */
            string uploadFile = Path.GetTempFileName();
            DirectoryInfo explodeDir = Utility.GetTempDirectory();

            ProcessPath(path, explodeDir);

            try
            {
                var resources = new List<Resource>();
                ulong totalSize = AddDirectoryToResources(resources, explodeDir, explodeDir.FullName);

                if (false == resources.IsNullOrEmpty())
                {
                    Resource[] appcloudResources = null;
                    if (totalSize > (64 * 1024))
                    {
                        appcloudResources = CheckResources(resources.ToArray());
                    }
                    if (appcloudResources.IsNullOrEmpty())
                    {
                        appcloudResources = resources.ToArrayOrNull();
                    }
                    else
                    {
                        foreach (Resource r in appcloudResources)
                        {
                            string localPath = Path.Combine(explodeDir.FullName, r.FN);
                            var localFileInfo = new FileInfo(localPath);
                            localFileInfo.Delete();
                            resources.Remove(r);
                        }
                    }
                    if (resources.IsNullOrEmpty())
                    {
                        /*
                            If no resource needs to be sent, add an empty file to ensure we have
                            a multi-part request that is expected by nginx fronting the CC.
                         */
                        File.WriteAllText(Path.Combine(explodeDir.FullName, ".__empty__"), String.Empty);
                    }

                    var zipper = new FastZip();
                    zipper.CreateZip(uploadFile, explodeDir.FullName, true, String.Empty);
                    var request = base.BuildVcapJsonRequest(Method.POST, Constants.APPS_PATH, name, "application");
                    request.AddFile("application", uploadFile);
                    request.AddParameter("_method", "put");
                    request.AddParameter("resources", JsonConvert.SerializeObject(appcloudResources.ToArrayOrNull()));
                    IRestResponse response = request.Execute();
                }
            }
            finally
            {
                Directory.Delete(explodeDir.FullName, true);
                File.Delete(uploadFile);
            }
        }

        private Resource[] CheckResources(Resource[] resourceAry)
        {
            /*
                Send in a resources manifest array to the system to have
                it check what is needed to actually send. Returns array
                indicating what is needed. This returned manifest should be
                sent in with the upload if resources were removed.
                E.g. [{:sha1 => xxx, :size => xxx, :fn => filename}]
             */
            var r = base.BuildVcapJsonRequest(Method.POST, Constants.RESOURCES_PATH);
            r.AddBody(resourceAry);
            IRestResponse response = r.Execute();
            return JsonConvert.DeserializeObject<Resource[]>(response.Content);
        }

        private static ulong AddDirectoryToResources(
            ICollection<Resource> resources, DirectoryInfo directory, string rootFullName)
        {
            ulong totalSize = 0;

            var fileTrimStartChars = new[] { '\\', '/' };

            foreach (FileInfo file in directory.GetFiles())
            {
                totalSize += (ulong)file.Length;

                string hash     = GenerateHash(file.FullName);
                string filename = file.FullName;
                // The root path should be stripped. This is used
                // by the server to tar up the file that gets pushed
                // to the DEA.
                filename = filename.Replace(rootFullName, String.Empty);
                filename = filename.TrimStart(fileTrimStartChars);
                filename = filename.Replace('\\', '/');
                resources.Add(new Resource((ulong)file.Length, hash, filename));
            }

            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
            {
                totalSize += AddDirectoryToResources(resources, subdirectory, rootFullName);
            }

            return totalSize;
        }

        private static string GenerateHash(string fileName)
        {
            using (FileStream fs = File.OpenRead(fileName))
            {
                using (var sha1 = new SHA1Managed())
                {
                    return BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", String.Empty).ToLowerInvariant();
                }
            }
        }

        private static void ProcessPath(DirectoryInfo path, DirectoryInfo explodeDir)
        {
            var warFile = Directory.EnumerateFiles(path.FullName, "*.war", SearchOption.TopDirectoryOnly).FirstOrDefault();
            var zipFile = Directory.EnumerateFiles(path.FullName, "*.zip", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (File.Exists(warFile))
            {
                var unzip = new FastZip();
                unzip.ExtractZip(warFile, explodeDir.FullName, null);
            }
            else if (File.Exists(zipFile))
            {
                var unzip = new FastZip();
                unzip.ExtractZip(zipFile, explodeDir.FullName, null);
            }
            else
            {
                Utility.CopyDirectory(path, explodeDir);
            }
        }
    }
}