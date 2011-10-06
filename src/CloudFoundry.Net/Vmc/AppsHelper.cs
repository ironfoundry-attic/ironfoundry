namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;
    using CloudFoundry.Net.Types;
    using ICSharpCode.SharpZipLib.Zip;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;

    internal class AppsHelper : BaseVmcHelper
    {
        public AppsHelper(VcapCredentialManager credMgr) : base(credMgr) { }

        public void Start(Application argApplication)
        {
            Application app = GetApplication(argApplication.Name);
            if (false == app.Started)
            {
                argApplication.State = VcapStates.STARTED;
                UpdateApplication(argApplication);
                // NB: Ruby vmc does a LOT more steps here
                // TODO wait for start?
                isStarted(argApplication.Name);
            }
        }

        public void Stop(Application argApplication)
        {
            Application app = GetApplication(argApplication.Name);
            if (false == app.Stopped)
            {
                argApplication.State = VcapStates.STOPPED;
                UpdateApplication(argApplication);
            }
        }

        public string GetApplicationJson(string argName)
        {
            var r = new VcapRequest(credMgr, Constants.APPS_PATH, argName);
            return r.Execute().Content;
        }

        public Application GetApplication(string argName)
        {
            string json = GetApplicationJson(argName);
            return JsonConvert.DeserializeObject<Application>(json);
        }

        public VcapResponse UpdateApplication(Application argApp)
        {
            var r = new VcapJsonRequest(credMgr, Method.PUT, argApp, Constants.APPS_PATH, argApp.Name);
            return r.Execute<VcapResponse>();
        }

        public void Delete(string argName)
        {
            var r = new VcapJsonRequest(credMgr, Method.DELETE, Constants.APPS_PATH, argName);
            r.Execute();
        }

        public void Restart(Application argApp)
        {
            Stop(argApp);
            Start(argApp);
        }

        public VcapClientResult Push(string argName, string argDeployFQDN, ushort argInstances,
            DirectoryInfo argPath, uint argMemoryMB, string[] argProvisionedServiceNames, string argFramework, string argRuntime)
        {
            VcapClientResult rv;

            if (argPath == null)
            {
                rv = new VcapClientResult(false, "Application local location is needed");
            }
            else if (argDeployFQDN == null)
            {
                rv = new VcapClientResult(false, "Please specify the url to deploy as.");
            }
            else if (argFramework == null)
            {
                rv = new VcapClientResult(false, "Please specify application framework");
            }
            else
            {
                /*
                 * Before creating the app, ensure we can build resource list
                 */
                var resources = new List<Resource>();
                addDirectoryToResources(resources, argPath, argPath.FullName);

                var manifest = new AppManifest
                {
                    Name = argName,
                    Staging = new Staging { Framework = argFramework, Runtime = argRuntime },
                    Uris = new string[] { argDeployFQDN },
                    Instances = argInstances,
                    Resources = new AppResources { Memory = argMemoryMB },
                };

                var r = new VcapJsonRequest(credMgr, Method.POST, manifest, Constants.APPS_PATH);
                RestResponse response = r.Execute();

                Resource[] resourceAry = resources.ToArrayOrNull();

                r = new VcapJsonRequest(credMgr, Method.POST, resourceAry, Constants.RESOURCES_PATH);
                response = r.Execute();

                string tempFile = Path.GetTempFileName();
                try
                {
                    var zipper = new FastZip();
                    zipper.CreateZip(tempFile, argPath.FullName, true, String.Empty);

                    r = new VcapJsonRequest(credMgr, Method.POST, Constants.APPS_PATH, argName, "application");
                    r.AddParameter("_method", "put");
                    r.AddFile("application", tempFile);
                    r.AddParameter("resources", JsonConvert.SerializeObject(resourceAry));
                    response = r.Execute();
                }
                finally
                {
                    File.Delete(tempFile);
                }

                string app = GetApplicationJson(argName);
                JObject getInfo = JObject.Parse(app);
                string appName = (string)getInfo["name"];
                getInfo["state"] = VcapStates.STARTED;

                r = new VcapJsonRequest(credMgr, Method.PUT, getInfo, Constants.APPS_PATH, argName);
                response = r.Execute();

                bool started = isStarted(appName);

                if (started && false == argProvisionedServiceNames.IsNullOrEmpty())
                {
                    foreach (string svcName in argProvisionedServiceNames)
                    {
                        var servicesHelper = new ServicesHelper(credMgr);
                        servicesHelper.BindService(svcName, appName);
                    }
                }

                rv = new VcapClientResult(started);
            }

            return rv;
        }

        public string GetAppCrash(string argName)
        {
            var r = new VcapRequest(credMgr, Constants.APPS_PATH, argName, "crashes");
            return r.Execute().Content;
        }

        public IEnumerable<Crash> GetAppCrash(Application argApp)
        {
            var r = new VcapRequest(credMgr, Constants.APPS_PATH, argApp.Name, "crashes");
            return r.Execute<Crash[]>();
        }

        public IEnumerable<Application> ListApps(Cloud argCloud)
        {
            var r = new VcapRequest(credMgr, Constants.APPS_PATH);
            IEnumerable<Application> rv = r.Execute<List<Application>>();
            if (null != rv)
            {
                foreach (Application app in rv)
                {
                    app.Parent = argCloud; // TODO
                }
            }

            return rv;
        }

        private bool isStarted(string argName)
        {
            bool started = false;

            for (int i = 0; i < 5; ++i)
            {
                string appJson = GetApplicationJson(argName);
                JObject parsed = JObject.Parse(appJson);

                // Ruby detects health a little differently
                string appState         = (string)parsed["state"];
                ushort instances        = (ushort)parsed["instances"];
                var running = parsed["runningInstances"];
                
                ushort runningInstances = ( running.Type == JTokenType.Null) ? (ushort) 0 : (ushort)running;

                if (appState == VcapStates.STARTED && (instances == runningInstances))
                {
                    started = true;
                    break;
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(6));
                }
            }

            return started;
        }

        private static void addDirectoryToResources(List<Resource> argResources, DirectoryInfo argDirectory, string argRootFullName)
        {
            var fileTrimStartChars = new[] { '\\', '/' };

            foreach (FileInfo file in argDirectory.GetFiles())
            {
                string hash     = generateHash(file.FullName);
                long size       = file.Length;
                string filename = file.FullName;
                // The root path should be stripped. This is used
                // by the server to TAR up the file that gets pushed
                // to the DEA.
                filename = filename.Replace(argRootFullName, String.Empty);
                filename = filename.TrimStart(fileTrimStartChars);
                filename = filename.Replace('\\', '/');
                argResources.Add(new Resource((ulong)file.Length, hash, filename));
            }

            foreach (DirectoryInfo subdirectory in argDirectory.GetDirectories())
            {
                addDirectoryToResources(argResources, subdirectory, argRootFullName);
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