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

    public class Apps
    {
        private readonly string token;

        public Apps(string argToken)
        {
            token = argToken;
        }

        public string StartApp(string appname, string url)
        {
            if (url == null)
            {
                return ("Target URL has to be set");
            }
            else
            {
                var client = new RestClient();
                client.BaseUrl = url;
                var request = new RestRequest();
                request.Method = Method.PUT;
                request.Resource = "/apps/" + appname;
                request.AddHeader("Authorization", token);
                return client.Execute(request).Content;
            }
        }              

        public void StartApp(Application application, Cloud cloud)
        {
            application.State = Instance.InstanceState.STARTED;
            UpdateApplicationSettings(application, cloud);
        }

        public string GetAppInfo(string argName, string argUrl)
        {
            if (argUrl == null)
            {
                return ("Target URL has to be set");
            }
            else
            {
                var client = new RestClient();
                client.BaseUrl = argUrl;
                var request = new RestRequest();
                request.Method = Method.GET;
                request.Resource = Constants.APPS_PATH + "/" + argName;
                request.AddHeader("AUTHORIZATION", token);
                return client.Execute(request).Content;
            }
        }

        public Application GetAppInfo(String appname, Cloud cloud)
        {
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = "/apps/" + appname;
            request.AddHeader("Authorization", cloud.AccessToken);
            request.RequestFormat = DataFormat.Json;
            return (Application)JsonConvert.DeserializeObject(client.Execute(request).Content, typeof(Application));
        }

        public void StopApp(Application application, Cloud cloud)
        {
            application.State = Instance.InstanceState.STOPPED;
            UpdateApplicationSettings(application, cloud);
        }

        public VcapResponse UpdateApplicationSettings(Application application, Cloud cloud)
        {
            VcapResponse vmcResponse = null;
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.RequestFormat = DataFormat.Json;
            request.Method = Method.PUT;
            request.Resource = "/apps/" + application.Name;
            request.AddHeader("Authorization", cloud.AccessToken);
            request.AddBody(application);
            var response = client.Execute(request).Content;
            if (!String.IsNullOrEmpty(response.Trim()))
                vmcResponse = JsonConvert.DeserializeObject<VcapResponse>(response);
            return vmcResponse;
        }

        public string DeleteApp(string appname, string url, string accesstoken)
        {
            if (url == null)
            {
                return ("Target URL has to be set");
            }
            else if (accesstoken == null)
            {
                return ("Please login first");
            }
            else
            {
                var client = new RestClient();
                client.BaseUrl = url;
                var request = new RestRequest();
                request.Method = Method.DELETE;
                request.Resource = "/apps/" + appname;
                request.AddHeader("Authorization", accesstoken);
                return client.Execute(request).Content;
            }
        }

        public void DeleteApp(Application application, Cloud cloud)
        {
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.DELETE;
            request.Resource = "/apps/" + application.Name;
            request.AddHeader("Authorization", cloud.AccessToken);
            client.Execute(request);
        }

        public void RestartApp(Application application, Cloud cloud)
        {
            StopApp(application, cloud);
            StartApp(application, cloud);
        }

        public string Push(string argName, Uri argUri, DirectoryInfo argPath, string argDeployUrl,
            string argFramework, string argRuntime, uint argMemoryReservation, string argServiceBindings)
        {
            if (argUri == null)
            {
                return ("Target URL has to be set");
            }
            else if (argPath == null)
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
                if (argServiceBindings == null)
                {
                    argServiceBindings = "none";
                }

                //Try and create application
                //{"name":"johndoe","staging":{"framework":"sinatra","runtime":null},"uris":["johndoe.cloudfoundry.com"],"instances":1,"resources":{"memory":128}}
                var manifest = new AppManifest
                {
                    Name = argName,
                    Staging = new Staging { Framework = argFramework, Runtime = argRuntime },
                    Uris = new string[] { argDeployUrl },
                    Instances = 1,
                    Resources = new Resources { Memory = argMemoryReservation },
                };

                var request = new RestRequest
                {
                    Method = Method.POST,
                    RequestFormat = DataFormat.Json,
                    Resource = Constants.APPS_PATH,
                };
                request.AddBody(manifest);

                var client = new RestClient
                {
                    BaseUrl = argUri.AbsoluteUri,
                    FollowRedirects = false,
                };
                client.AddDefaultHeader("AUTHORIZATION", token);
                RestResponse response = client.Execute(request); // TODO process response

                var resourcesForPost = new List<Resource>();
                addDirectoryToResources(resourcesForPost, argPath, argPath.FullName);
                Resource[] resources = resourcesForPost.ToArray();

                client = new RestClient
                {
                    BaseUrl = argUri.AbsoluteUri,
                    FollowRedirects = false,
                };
                client.AddDefaultHeader("AUTHORIZATION", token);

                request = new RestRequest
                {
                    Method = Method.POST,
                    RequestFormat = DataFormat.Json,
                    Resource = Constants.RESOURCES_PATH,
                };
                request.AddHeader("AUTHORIZATION", token);
                request.AddBody(resources);
                response = client.Execute(request); // TODO process response

                // This is required in order to pass the JSON as a parameter
                string resourcesJson = JsonConvert.SerializeObject(resources);

                client = new RestClient
                {
                    BaseUrl = argUri.AbsoluteUri,
                    FollowRedirects = false,
                };
                client.AddDefaultHeader("AUTHORIZATION", token);

                string tempFile = Path.GetTempFileName();
                try
                {
                    var zipper = new FastZip();
                    zipper.CreateZip(tempFile, argPath.FullName, true, String.Empty);

                    request = new RestRequest
                    {
                        Method = Method.POST,
                        Resource = Constants.APPS_PATH + "/" + argName + "/application",
                    };
                    request.AddHeader("AUTHORIZATION", token);
                    request.AddParameter("_method", "put");
                    request.AddFile("application", tempFile);
                    request.AddParameter("resources", resourcesJson);
                    response = client.Execute(request);
                }
                finally
                {
                    File.Delete(tempFile);
                }

                string app = GetAppInfo(argName, argUri.AbsoluteUri);
                JObject getInfo = JObject.Parse(app);
                getInfo["state"] = "STARTED";

                client = new RestClient
                {
                    BaseUrl = argUri.AbsoluteUri,
                    FollowRedirects = false,
                };
                client.AddDefaultHeader("AUTHORIZATION", token);

                request = new RestRequest
                {
                    Method = Method.PUT,
                    Resource = Constants.APPS_PATH + "/" + argName,
                    RequestFormat = DataFormat.Json,
                };
                request.AddHeader("AUTHORIZATION", token);
                request.AddBody(getInfo);
                response = client.Execute(request);

                string info = string.Empty;
                for (int i = 0; i < 4; i++)
                {
                    info = GetAppInfo(argName, argUri.AbsoluteUri);
                    var crash = GetAppCrash(argName, argUri.AbsoluteUri);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                return info;

            }
        }

        private void addDirectoryToResources(List<Resource> resource, DirectoryInfo directory, string rootFullName)
        {
            foreach (var file in directory.GetFiles())
            {
                var hash = GenerateHash(file.FullName);
                var size = file.Length;
                var filename = file.FullName;
                // The root path should be stripped. This is used
                // by the server to TAR up the file that gets pushed
                // to the DEA.
                filename = filename.Replace(rootFullName, string.Empty);
                resource.Add(new Resource() { Size = (ulong)file.Length, SHA1 = hash, FN = filename });
            }

            foreach (var subdirectory in directory.GetDirectories())
                addDirectoryToResources(resource, subdirectory, rootFullName);
        }

        public string GetAppCrash(string appname, string url)
        {
            if (url == null)
            {
                return ("Target URL has to be set");
            }
            else
            {
                var client = new RestClient();
                client.BaseUrl = url;
                var request = new RestRequest();
                request.Method = Method.GET;
                request.Resource = Constants.APPS_PATH + "/" + appname + "/crashes";
                request.AddHeader("AUTHORIZATION", token);
                return client.Execute(request).Content;
            }
        }

        public List<Crash> GetAppCrash (Application application, Cloud cloud){
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = "/apps/" + application.Name + "/crashes";
            request.AddHeader("Authorization", cloud.AccessToken);
            return (List<Crash>)JsonConvert.DeserializeObject(client.Execute(request).Content, typeof(List<Crash>));
        }

        public string ListApps (string url, string accesstoken)
        {
            if (url == null)
            {
                return ("Target URL has to be set");
            } 
            else if (accesstoken == null)
            {
                return ("Please login first");
            }
            else
            {
                var client = new RestClient();
                client.BaseUrl = url;
                var request = new RestRequest();
                request.Method = Method.GET;
                request.Resource = "/apps";
                request.AddHeader("Authorization", accesstoken);
                return client.Execute(request).Content;
            }
        }

        public List<Application> ListApps (Cloud cloud){
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = "/apps";
            request.AddHeader("Authorization", cloud.AccessToken);
            var response = client.Execute(request).Content;
            var list = JsonConvert.DeserializeObject<List<Application>>(response);
            list.ForEach((a) => a.Parent = cloud);
            return list;
        }

        public static string GenerateHash(string filePathAndName)
        {
            string hashText = "";
            string hexValue = "";

            byte[] fileData = File.ReadAllBytes(filePathAndName);
            byte[] hashData = SHA1.Create().ComputeHash(fileData); // SHA1 or MD5

            foreach (byte b in hashData)
            {
                hexValue = b.ToString("X").ToLower(); // Lowercase for compatibility on case-sensitive systems
                hashText += (hexValue.Length == 1 ? "0" : "") + hexValue;
            }

            return hashText;
        }


    }

}
