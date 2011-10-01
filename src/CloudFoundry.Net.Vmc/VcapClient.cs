namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json.Linq;
    using Properties;
    using RestSharp;
    using Types;

    public class VcapClient : IVcapClient
    {
        public string URL { get; private set; }

        // {"http://api.vcap.me":"04085b0849221a6c756b652e62616b6b656e4074696572332e636f6d063a0645546c2b078b728b4e2219000104ec0d65746833caddd87eac48b0b2989604"}
        public string AccessToken { get; private set; }

        public VcapClient()
        {
            URL = readTargetFile();
            AccessToken = String.Empty; // TODO
        }

        public VcapClient(string argURL)
        {
            URL = argURL;
        }

        public VcapClientResult LogIn(string email, string password)
        {
            var cfa = new VmcAdministration();

            string result = cfa.Login(email, password, URL);

            JObject parsed = JObject.Parse(result);
            AccessToken = Convert.ToString(parsed["token"]);

            return new VcapClientResult();
        }

        public VcapClientResult LogIn(Cloud cloud)
        {
            VmcAdministration cfa = new VmcAdministration();
            return new VcapClientResult(cfa.Login(cloud));
        }
        
        public string Push(string appname, string fdqn, string fileURI, string framework, string memorysize)
        {
            VmcApps cfapps = new VmcApps();
            var app =  cfapps.PushApp(appname, URL, AccessToken, fileURI, fdqn, framework, null,memorysize, null);
            return app;
        }

        public VcapClientResult Info()
        {
            VcapClientResult rv;

            rv = checkTarget();
            if (rv.Success)
            {
                if (validAccessToken)
                {
                    JObject obj = JObject.Parse(AccessToken);
                    VcapClient cfm = new VcapClient();
                    cfm.AccessToken = (string)obj.Value<string>(URL);
                    cfm.URL = URL;
                    Console.WriteLine(cfm.Info());
                }
                else
                {
                    VcapClient cfm = new VcapClient();
                    cfm.URL = URL;
                    Console.WriteLine(cfm.Info());
                }

                var client = new RestClient { BaseUrl = URL };
                var request = new RestRequest { Resource = "/info" };
                if (AccessToken != null)
                {
                    request.AddHeader("Authorization", AccessToken);
                }
                // TODO return client.Execute(request).Content;
            }

            // VmcInit init = new VmcInit();
            // return init.Info(AccessToken,URL);

            return new VcapClientResult();
        }

        private bool validAccessToken
        {
            get { return false == AccessToken.IsNullOrWhiteSpace(); }
        }

        private VcapClientResult checkTarget()
        {
            VcapClientResult rv;

            if (URL.IsNullOrWhiteSpace())
            {
                rv = new VcapClientResult(false, Resources.VcapClient_PleaseSetTarget_Message);
            }
            else
            {
                rv = new VcapClientResult();
            }

            return rv;
        }


        public void StopApp(Application application, Cloud cloud)
        {
            VmcApps apps = new VmcApps();
            apps.StopApp(application, cloud);
        }

        public void StartApp(Application application, Cloud cloud)
        {
            VmcApps apps = new VmcApps();
            apps.StartApp(application, cloud);
        }

        public Application GetAppInfo(string appname, Cloud cloud)
        {
            VmcApps app = new VmcApps();
            return app.GetAppInfo(appname, cloud);
        }

        public VcapResponse UpdateApplicationSettings(Application application, Cloud cloud)
        {
            VmcApps app = new VmcApps();
            return app.UpdateApplicationSettings(application, cloud);
        }


        public void RestartApp(Application application, Cloud cloud)
        {
            VmcApps app = new VmcApps();
            app.RestartApp(application, cloud);
        }


        public string GetLogs(Application application, int instanceNumber, Cloud cloud)
        {
            VmcInfo info = new VmcInfo();
            return info.GetLogs(application, instanceNumber, cloud);
        }

        public SortedDictionary<int,StatInfo> GetStats(Application application, Cloud cloud)
        {
            VmcInfo info = new VmcInfo();
            return info.GetStats(application, cloud);
        }

        public List<ExternalInstance> GetInstances(Application application, Cloud cloud)
        {
            VmcInfo info = new VmcInfo();
            return info.GetInstances(application, cloud);
        }

        public List<Crash> GetAppCrash(Application application, Cloud cloud)
        {
            VmcApps apps = new VmcApps();
            return apps.GetAppCrash(application, cloud);
        }

        public List<Application> ListApps(Cloud cloud)
        {
            VmcApps apps = new VmcApps();
            return apps.ListApps(cloud);
        }

        public List<SystemServices> GetAvailableServices(Cloud cloud)
        {
            VmcServices services = new VmcServices();
            return services.GetAvailableServices(cloud);
        }

        public List<AppService> GetProvisionedServices(Cloud cloud)
        {
            VmcServices services = new VmcServices();
            return services.GetProvisionedServices(cloud);
        }

        private static string readTargetFile()
        {
            string infile = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), ".vmc_target");
            return File.ReadAllText(infile);
        }

        private static void writeTargetFile(string target)
        {
            string outfile = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), ".vmc_target");
            File.WriteAllText(outfile, target);
        }
    }
}
