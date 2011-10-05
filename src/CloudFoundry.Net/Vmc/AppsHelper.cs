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
            argApplication.State = Instance.InstanceState.STARTED;
            UpdateApplicationSettings(argApplication);
        }

        public void Stop(Application argApplication)
        {
            argApplication.State = Instance.InstanceState.STOPPED;
            UpdateApplicationSettings(argApplication);
        }

        public string GetAppInfoJson(string argName)
        {
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.GET, Constants.APPS_PATH, argName);
            return client.Execute(request).Content;
        }

        public Application GetAppInfo(string argName)
        {
            string json = GetAppInfoJson(argName);
            return JsonConvert.DeserializeObject<Application>(json);
        }

        public VcapResponse UpdateApplicationSettings(Application argApp)
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

        public string Push(string argName, DirectoryInfo argPath, string argDeployUrl,
            string argFramework, string argRuntime, uint argMemoryReservation, string argServiceBindings)
        {
            if (argPath == null)
            {
                return ("Application local location is needed");
            }
            else if (argDeployUrl == null)
            {
                return ("Please specify the url to deploy as.");
            }
            else if (argFramework == null)
            {
                return ("Please specify application framework");
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

                // This is required in order to pass the JSON as a parameter
                string resourcesJson = JsonConvert.SerializeObject(resources.ToArrayOrNull());

                client = buildClient();
                request = buildRequest(Method.POST, DataFormat.Json, Constants.RESOURCES_PATH);
                request.AddBody(resourcesJson);
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
                    request.AddParameter("resources", resourcesJson);

                    response = executeRequest(client, request);
                }
                finally
                {
                    File.Delete(tempFile);
                }

                string app = GetAppInfoJson(argName);
                JObject getInfo = JObject.Parse(app);
                getInfo["state"] = "STARTED";

                client = buildClient();

                request = buildRequest(Method.PUT, DataFormat.Json, Constants.APPS_PATH, argName);
                request.AddBody(getInfo);
                response = client.Execute(request);

                string info = null;
                for (int i = 0; i < 4; i++)
                {
                    info = GetAppInfoJson(argName);
                    var crash = GetAppCrash(argName);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

                return info;
            }
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

            foreach (var subdirectory in argDirectory.GetDirectories())
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