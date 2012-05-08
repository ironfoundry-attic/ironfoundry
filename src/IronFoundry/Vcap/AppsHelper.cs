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
    using IronFoundry.Properties;
    using IronFoundry.Types;
    using Newtonsoft.Json;
    using RestSharp;

    internal class AppsHelper : BaseVmcHelper
    {
        public AppsHelper(VcapCredentialManager credMgr) : base(credMgr) { }

        public void Start(string applicationName)
        {
            Application app = GetApplication(applicationName);
            if (false == app.IsStarted)
            {
                app.Start();
                UpdateApplication(app);
                IsStarted(app.Name);
            }
        }

        public void Start(Application application)
        {
            Start(application.Name);
        }

        public void Stop(string applicationName)
        {
            Application app = GetApplication(applicationName);
            if (false == app.IsStopped)
            {
                app.Stop();
                UpdateApplication(app);
            }
        }

        public void Stop(Application application)
        {
            Stop(application.Name);
        }

        public VcapResponse UpdateApplication(Application app)
        {
            var r = new VcapJsonRequest(credMgr, Method.PUT, Constants.APPS_PATH, app.Name);
            r.AddBody(app);
            return r.Execute<VcapResponse>();
        }

        public void Delete(string name)
        {
            var r = new VcapJsonRequest(credMgr, Method.DELETE, Constants.APPS_PATH, name);
            r.Execute();
        }

        public void Restart(string appName)
        {
            Stop(appName);
            Start(appName);
        }

        public void Restart(Application app)
        {
            Stop(app);
            Start(app);
        }

        public byte[] Files(string name, string path, ushort instance)
        {
            var r = new VcapRequest(credMgr, Constants.APPS_PATH, name, "instances", instance, "files", path);
            RestResponse response = r.Execute();
            return response.RawBytes;
        }

        public VcapClientResult Push(
            string name, string deployFQDN, ushort instances,
            DirectoryInfo path, uint memoryMB, string[] provisionedServiceNames)
        {
            VcapClientResult rv;

            if (path == null)
            {
                return new VcapClientResult(false, "Application local location is needed");
            }

            if (deployFQDN == null)
            {
                return new VcapClientResult(false, "Please specify the url to deploy as.");
            }

            DetetectedFramework framework = FrameworkDetetctor.Detect(path);
            if (framework == null)
            {
                rv = new VcapClientResult(false, "Please specify application framework");
            }
            else
            {
                if (AppExists(name))
                {
                    rv = new VcapClientResult(false, String.Format(Resources.AppsHelper_PushApplicationExists_Fmt, name));
                }
                else
                {
                    /*
                     * Before creating the app, ensure we can build resource list
                     */
                    var resources = new List<Resource>();
                    ulong totalSize = AddDirectoryToResources(resources, path, path.FullName);

                    var manifest = new AppManifest
                    {
                        Name = name,
                        Staging = new Staging { Framework = framework.Framework, Runtime = framework.Runtime },
                        Uris = new string[] { deployFQDN },
                        Instances = instances,
                        Resources = new AppResources { Memory = memoryMB },
                    };

                    var r = new VcapJsonRequest(credMgr, Method.POST, Constants.APPS_PATH);
                    r.AddBody(manifest);
                    RestResponse response = r.Execute();

                    UploadAppBits(name, path);

                    Application app = GetApplication(name);
                    app.Start();
                    r = new VcapJsonRequest(credMgr, Method.PUT, Constants.APPS_PATH, name);
                    r.AddBody(app);
                    response = r.Execute();

                    bool started = IsStarted(app.Name);

                    if (started && false == provisionedServiceNames.IsNullOrEmpty())
                    {
                        foreach (string svcName in provisionedServiceNames)
                        {
                            var servicesHelper = new ServicesHelper(credMgr);
                            servicesHelper.BindService(svcName, app.Name);
                        }
                    }

                    rv = new VcapClientResult(started);
                }
            }

            return rv;
        }

        public VcapClientResult Update(string name, DirectoryInfo path)
        {
            VcapClientResult rv;

            if (path == null)
            {
                rv = new VcapClientResult(false, "Application local location is needed");
            }
            else
            {
                UploadAppBits(name, path);
                Application app = GetApplication(name);
                Restart(app);
                rv = new VcapClientResult();
            }

            return rv;
        }

        public string GetAppCrash(string name)
        {
            var r = new VcapRequest(credMgr, Constants.APPS_PATH, name, "crashes");
            return r.Execute().Content;
        }

        public IEnumerable<Crash> GetAppCrash(Application app)
        {
            var r = new VcapRequest(credMgr, Constants.APPS_PATH, app.Name, "crashes");
            return r.Execute<Crash[]>();
        }

        public IEnumerable<Application> GetApplications(VcapUser user)
        {
            return base.GetApplications(user.Email);
        }

        private bool IsStarted(string name)
        {
            bool started = false;

            for (int i = 0; i < 20; ++i)
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
                    Thread.Sleep(TimeSpan.FromSeconds(3));
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
                    var request = new VcapJsonRequest(credMgr, Method.PUT, Constants.APPS_PATH, name, "application");
                    request.AddFile("application", uploadFile);
                    request.AddParameter("resources", JsonConvert.SerializeObject(appcloudResources.ToArrayOrNull()));
                    RestResponse response = request.Execute();
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
            var r = new VcapJsonRequest(credMgr, Method.POST, Constants.RESOURCES_PATH);
            r.AddBody(resourceAry);
            RestResponse response = r.Execute();
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