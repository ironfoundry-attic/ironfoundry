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

        public void Start(string argApplicationName)
        {
            Application app = GetApplication(argApplicationName);
            if (false == app.Started)
            {
                app.State = VcapStates.STARTED;
                UpdateApplication(app);
                // NB: Ruby vmc does a LOT more steps here
                // TODO wait for start?
                isStarted(app.Name);
            }
        }

        public void Start(Application argApplication)
        {
            Start(argApplication.Name);
        }

        public void Stop(string argApplicationName)
        {
            Application app = GetApplication(argApplicationName);
            if (false == app.Stopped)
            {
                app.State = VcapStates.STOPPED;
                UpdateApplication(app);
            }
        }

        public void Stop(Application argApplication)
        {
            Stop(argApplication.Name);
        }

        public VcapResponse UpdateApplication(Application app)
        {
            var r = new VcapJsonRequest(credMgr, Method.PUT, Constants.APPS_PATH, app.Name);
            r.AddBody(app);
            return r.Execute<VcapResponse>();
        }

        public void Delete(string argName)
        {
            var r = new VcapJsonRequest(credMgr, Method.DELETE, Constants.APPS_PATH, argName);
            r.Execute();
        }

        public void Restart(string argAppName)
        {
            Stop(argAppName);
            Start(argAppName);
        }

        public void Restart(Application argApp)
        {
            Stop(argApp);
            Start(argApp);
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

                    Resource[] resourceAry = resources.ToArrayOrNull();

                    r = new VcapJsonRequest(credMgr, Method.POST, Constants.RESOURCES_PATH);
                    r.AddBody(resourceAry);
                    response = r.Execute();

                    string tempFile = Path.GetTempFileName();
                    try
                    {
                        var zipper = new FastZip();
                        zipper.CreateZip(tempFile, path.FullName, true, String.Empty);

                        r = new VcapJsonRequest(credMgr, Method.POST, Constants.APPS_PATH, name, "application");
                        r.AddParameter("_method", "put");
                        r.AddFile("application", tempFile);
                        r.AddParameter("resources", JsonConvert.SerializeObject(resourceAry));
                        response = r.Execute();
                    }
                    finally
                    {
                        File.Delete(tempFile);
                    }

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