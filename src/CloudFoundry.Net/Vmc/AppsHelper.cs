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
        public AppsHelper(VcapCredentialManager argCredentialManager)
            : base(argCredentialManager) { }

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
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.GET, Constants.APPS_PATH, argName);
            return client.Execute(request).Content;
        }

        public Application GetApplication(string argName)
        {
            string json = GetApplicationJson(argName);
            return JsonConvert.DeserializeObject<Application>(json);
        }

        public VcapResponse UpdateApplication(Application argApp)
        {
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.PUT, DataFormat.Json, Constants.APPS_PATH, argApp.Name);
            request.AddBody(argApp);
            return executeRequest<VcapResponse>(client, request);
        }

        public void Delete(string argName)
        {
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.DELETE, Constants.APPS_PATH, argName);
            executeRequest(client, request);
        }

        public void Restart(Application argApp)
        {
            Stop(argApp);
            Start(argApp);
        }

        public VcapClientResult Push(string argName, DirectoryInfo argPath, string argDeployUrl,
            string argFramework, string argRuntime, uint argMemoryReservation, string argServiceBindings)
        {
            VcapClientResult rv;

            if (argPath == null)
            {
                rv = new VcapClientResult(false, "Application local location is needed");
            }
            else if (argDeployUrl == null)
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
                    Uris = new string[] { argDeployUrl },
                    Instances = 1,
                    Resources = new AppResources { Memory = argMemoryReservation },
                };

                RestClient client = buildClient();
                RestRequest request = buildRequest(Method.POST, DataFormat.Json, Constants.APPS_PATH);
                request.AddBody(manifest);
                RestResponse response = executeRequest(client, request);

                Resource[] resourceAry = resources.ToArrayOrNull();

                client = buildClient();
                request = buildRequest(Method.POST, DataFormat.Json, Constants.RESOURCES_PATH);
                request.AddBody(resourceAry);
                response = executeRequest(client, request);

                client = buildClient();

                string tempFile = Path.GetTempFileName();
                try
                {
                    var zipper = new FastZip();
                    zipper.CreateZip(tempFile, argPath.FullName, true, String.Empty);

                    request = buildRequest(Method.POST, Constants.APPS_PATH, argName, "application");
                    request.AddParameter("_method", "put");
                    request.AddFile("application", tempFile);
                    // This is required in order to pass the JSON as a parameter
                    request.AddParameter("resources", JsonConvert.SerializeObject(resourceAry));

                    response = executeRequest(client, request);
                }
                finally
                {
                    File.Delete(tempFile);
                }

                string app = GetApplicationJson(argName);
                JObject getInfo = JObject.Parse(app);
                getInfo["state"] = "STARTED";

                client = buildClient();

                request = buildRequest(Method.PUT, DataFormat.Json, Constants.APPS_PATH, argName);
                request.AddBody(getInfo);
                response = client.Execute(request);

                bool started = isStarted(argName);

                rv = new VcapClientResult(started);
            }

            return rv;
        }

        public string GetAppCrash(string argName)
        {
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.GET, Constants.APPS_PATH, argName, "crashes");
            RestResponse response = executeRequest(client, request);
            return response.Content;
        }

        public IEnumerable<Crash> GetAppCrash(Application argApp)
        {
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.GET, Constants.APPS_PATH, argApp.Name, "crashes");
            return executeRequest<Crash[]>(client, request);
        }

        public IEnumerable<Application> ListApps(Cloud argCloud)
        {
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.GET, Constants.APPS_PATH);
            IEnumerable<Application> rv = executeRequest<List<Application>>(client, request);
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
                ushort runningInstances = (ushort)parsed["runningInstances"];

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