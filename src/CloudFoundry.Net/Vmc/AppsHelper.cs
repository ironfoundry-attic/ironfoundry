using CloudFoundry.Net.Extensions;

namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;
    using CloudFoundry.Net.Properties;
    using CloudFoundry.Net.Types;
    using ICSharpCode.SharpZipLib.Zip;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;

    internal class AppsHelper : BaseVmcHelper
    {
        public AppsHelper(VcapCredentialManager credMgr) : base(credMgr) { }

        public void Start(string applicationName)
        {
            Application app = GetApplication(applicationName);
            if (false == app.IsStarted)
            {
                app.State = VcapStates.STARTED;
                UpdateApplication(app);
                // NB: Ruby vmc does a LOT more steps here
                // TODO wait for start?
                isStarted(app.Name);
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
                app.State = VcapStates.STOPPED;
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

        public VcapClientResult Push(string name, string deployFQDN, ushort instances,
            DirectoryInfo path, uint memoryMB, string[] provisionedServiceNames, string framework, string runtime)
        {
            VcapClientResult rv;

            if (path == null)
            {
                rv = new VcapClientResult(false, "Application local location is needed");
            }
            else if (deployFQDN == null)
            {
                rv = new VcapClientResult(false, "Please specify the url to deploy as.");
            }
            else if (framework == null)
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
                    addDirectoryToResources(resources, path, path.FullName);

                    var manifest = new AppManifest
                    {
                        Name = name,
                        Staging = new Staging { Framework = framework, Runtime = runtime },
                        Uris = new string[] { deployFQDN },
                        Instances = instances,
                        Resources = new AppResources { Memory = memoryMB },
                    };

                    var r = new VcapJsonRequest(credMgr, Method.POST, Constants.APPS_PATH);
                    r.AddBody(manifest);
                    RestResponse response = r.Execute();

                    uploadAppBits(name, path);

                    Application app = GetApplication(name);
                    app.State = VcapStates.STARTED;
                    r = new VcapJsonRequest(credMgr, Method.PUT, Constants.APPS_PATH, name);
                    r.AddBody(app);
                    response = r.Execute();

                    bool started = isStarted(app.Name);

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
                uploadAppBits(name, path);
                Application app = GetApplication(name);
                if (app.IsStarted)
                {
                    Restart(app);
                }
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

        private bool isStarted(string name)
        {
            bool started = false;

            for (int i = 0; i < 20; ++i)
            {
                string appJson = GetApplicationJson(name);
                JObject parsed = JObject.Parse(appJson);

                // Ruby detects health a little differently
                string appState          = (string)parsed["state"];                
                ushort? runningInstances = (ushort?)parsed["runningInstances"];

                if ((appState == VcapStates.STARTED) &&
                    (runningInstances.HasValue) &&
                    (runningInstances.Value > 0))
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

        private void uploadAppBits(string name, DirectoryInfo path)
        {
            /*
             * Before creating the app, ensure we can build resource list
             */
            var resources = new List<Resource>();
            addDirectoryToResources(resources, path, path.FullName);
            Resource[] resourceAry = resources.ToArrayOrNull();

            var r = new VcapJsonRequest(credMgr, Method.POST, Constants.RESOURCES_PATH);
            r.AddBody(resourceAry);
            RestResponse response = r.Execute();
            // TODO only upload files that have changed

            string tempFile = Path.GetTempFileName();
            try
            {
                var zipper = new FastZip();
                zipper.CreateZip(tempFile, path.FullName, true, String.Empty);

                r = new VcapJsonRequest(credMgr, Method.PUT, Constants.APPS_PATH, name, "application");
                r.AddFile("application", tempFile);
                r.AddParameter("resources", JsonConvert.SerializeObject(resourceAry));
                response = r.Execute();
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        private static void addDirectoryToResources(ICollection<Resource> resources, DirectoryInfo directory, string rootFullName)
        {
            var fileTrimStartChars = new[] { '\\', '/' };

            foreach (FileInfo file in directory.GetFiles())
            {
                string hash     = generateHash(file.FullName);
                long size       = file.Length;
                string filename = file.FullName;
                // The root path should be stripped. This is used
                // by the server to TAR up the file that gets pushed
                // to the DEA.
                filename = filename.Replace(rootFullName, String.Empty);
                filename = filename.TrimStart(fileTrimStartChars);
                filename = filename.Replace('\\', '/');
                resources.Add(new Resource((ulong)file.Length, hash, filename));
            }

            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
            {
                addDirectoryToResources(resources, subdirectory, rootFullName);
            }
        }

        private static string generateHash(string fileName)
        {
            using (FileStream fs = File.OpenRead(fileName))
            {
                using (var sha1 = new SHA1Managed())
                {
                    return BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", String.Empty).ToLowerInvariant();
                }
            }
        }
    }
}