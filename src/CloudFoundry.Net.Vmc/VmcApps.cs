namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;
    using ICSharpCode.SharpZipLib.Zip;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using CloudFoundry.Net.Types;

    internal class VmcApps
    {
        public string StartApp (string appname, string url, string accesstoken) 
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
                request.Method = Method.PUT;
                request.Resource = "/apps/" + appname;
                request.AddHeader("Authorization", accesstoken);
                return client.Execute(request).Content;
            }
        }

        public void StartApp (Application application, Cloud cloud){
            
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.PUT;
            request.Resource = "/apps/"+application.Name;
            application.State = "Started";
            request.AddHeader("Authorization", cloud.AccessToken);
            request.AddObject(application);
            request.RequestFormat = DataFormat.Json;
            client.Execute(request);
        }

        
        public string GetAppInfo (string appname, string url, string accesstoken)
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
                request.Resource = "/apps/"+appname;
                request.AddHeader("Authorization", accesstoken);
                return client.Execute(request).Content;
            }
        }

        

        public Application GetAppInfo (String appname, Cloud cloud){
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
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.PUT;
            request.Resource = "/apps/" + application.Name;
            application.State = "Stopped";
            request.AddHeader("Authorization", cloud.AccessToken);
            request.AddObject(application);
            request.RequestFormat = DataFormat.Json;
            client.Execute(request);

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

        public void DeleteApp(Application application, Cloud cloud){
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.DELETE;
            request.Resource = "/apps/" + application.Name;
            request.AddHeader("Authorization", cloud.AccessToken);
            client.Execute(request);
        }

        public void RestartApp (Application application, Cloud cloud)
        {
            StopApp(application, cloud);
            StartApp(application, cloud);
        }

        public string PushApp (string Appname, string Url, string Accesstoken, string Dirlocation, string Deployedurl, string Framework, string Runtime, string Memoryreservation, string Servicebindings )
        {
            if (Url == null)
            {
                return ("Target URL has to be set");
            }
            else if (Accesstoken == null)
            {
                return ("Please login first");
            }
            else if (Dirlocation == null)
            {
                return ("Application local location is needed");
            }
            else if (Deployedurl == null)
            {
                return ("Please specify the url to deploy as.");
            }
            else if (Framework == null)
            {
                return ("Please specify application framework");
            }
            else if (Memoryreservation == null)
            {
                return ("Please specify size of memory to allocate to application");
            }
            else
            {                
                if (Servicebindings == null)
                    Servicebindings = "none";

                var client = new RestClient();
                client.BaseUrl = Url;
                
                var request = new RestRequest();
               // request.AddHeader("Authorization", Accesstoken);
                //Try and create application
                //{"name":"johndoe","staging":{"framework":"sinatra","runtime":null},"uris":["johndoe.cloudfoundry.com"],"instances":1,"resources":{"memory":128}}
                VmcApplication appdetails = new VmcApplication();
                appdetails.name = Appname;
                appdetails.staging = new staging { framework = Framework, runtime = Runtime };
                appdetails.uris = new string[] { Deployedurl };
                appdetails.instances = 1;
                appdetails.resources = new resources { memory = Convert.ToInt32(Memoryreservation) };


                JsonSerializer js = new JsonSerializer();
                request.Method = Method.POST;
                request.RequestFormat = DataFormat.Json;
                request.AddBody(appdetails);
                request.Resource = "/apps";
                client.AddDefaultHeader("Authorization", Accesstoken);
                client.FollowRedirects = false;
                client.Execute(request);

                List<Resource> resourcesForPost = new List<Resource>();
                DirectoryInfo di = new DirectoryInfo(Dirlocation);
                AddDirectoryToResources(resourcesForPost, di, di.FullName);
                var resources = resourcesForPost.ToArray();
     

                var client4 = new RestClient();
                client4.BaseUrl = Url;
                client4.AddDefaultHeader("Authorization", Accesstoken);
                client4.FollowRedirects = false;

                var request4 = new RestRequest();
                request4.Method = Method.POST;
                request4.RequestFormat = DataFormat.Json;
                request4.AddHeader("Authorization", Accesstoken);
                request4.Resource = "/resources";
                request4.AddBody(resources);
                var req4content = client4.Execute(request4);

                // This is required in order to pass the JSON as a parameter
                JsonSerializer serializer = new JsonSerializer();
                string resourcesJson = JsonConvert.SerializeObject(resources);

                var client2 = new RestClient();
                client2.BaseUrl = Url;
                client2.AddDefaultHeader("Authorization", Accesstoken);

                var request2 = new RestRequest();
                
                FastZip zipper = new FastZip();
                zipper.CreateZip(Environment.GetEnvironmentVariable("TEMP")+"\\"+Appname + ".zip", Dirlocation, true,"");

                request2.Method = Method.POST;
                request2.AddHeader("Authorization", Accesstoken);
                request2.Resource = "/apps/"+Appname+"/application";
                request2.AddParameter("_method", "put");
                request2.AddFile("application", (Environment.GetEnvironmentVariable("TEMP") + "\\" + Appname + ".zip"));
                request2.AddParameter("resources", resourcesJson);
                var content = client2.Execute(request2).Content;

                var app = GetAppInfo(Appname, Url, Accesstoken);
                JObject getInfo = JObject.Parse(app);
                getInfo["state"] = "STARTED";

                var client3 = new RestClient();
                client3.BaseUrl = Url;
                client3.AddDefaultHeader("Authorization", Accesstoken);
                var request3 = new RestRequest();
                request3.Method = Method.PUT;
                request3.Resource = "/apps/" + Appname;
                request3.RequestFormat = DataFormat.Json;
                request3.AddHeader("Authorization", Accesstoken);
                request3.AddBody(getInfo);
                client3.FollowRedirects = false;
                var resp = client3.Execute(request3).Content;

                string info = string.Empty;
                for (int i = 0; i < 4; i++)
                {
                    info = GetAppInfo(Appname, Url, Accesstoken);
                    var crash = GetAppCrash(Appname, Url, Accesstoken);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                return info;
            
            }
        }

        public void AddDirectoryToResources(List<Resource> resource,DirectoryInfo directory, string rootFullName)
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
                resource.Add(new Resource() { size = file.Length, sha1 = hash, fn = filename });
            }

            foreach (var subdirectory in directory.GetDirectories())
                AddDirectoryToResources(resource, subdirectory, rootFullName);
        }

       
        public string GetAppCrash (string appname, string url, string accesstoken)
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
                request.Resource = "/apps/" + appname + "/crashes";
                request.AddHeader("Authorization", accesstoken);
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
