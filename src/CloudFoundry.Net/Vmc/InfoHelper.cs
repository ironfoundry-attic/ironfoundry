namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CloudFoundry.Net.Types;
    using Newtonsoft.Json;
    using RestSharp;

    public class InfoHelper : BaseVmcHelper
    {
        public string GetLogs(Application application, int instanceNumber, Cloud cloud)
        {
            string logoutput = "";

            logoutput = "====stderr.log====\n";
            logoutput = logoutput + GetStdErrLog(application, instanceNumber, cloud);
            logoutput = logoutput + "\n====stdout.log====\n";
            logoutput = logoutput + GetStdOutLog(application, instanceNumber, cloud);
            logoutput = logoutput + "\n====startup.log====\n";
            logoutput = logoutput + GetStartupLog(application, instanceNumber, cloud);

            return logoutput;
        }

        public string GetStdErrLog(Application application, int instanceNumber, Cloud cloud) 
        {
            //GET /apps/sroytest1/instances/0/files/logs/stderr.log
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = "/apps/" + application.Name +"/"+instanceNumber+"/files/logs/stderr.log";
            request.AddHeader("Authorization", cloud.AccessToken);
            return client.Execute(request).Content;

        }

        public string GetStdOutLog(Application application, int instanceNumber, Cloud cloud)
        {
            //GET /apps/sroytest1/instances/0/files/logs/stdout.log
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = "/apps/" + application.Name + "/" + instanceNumber + "/files/logs/stdout.log";
            request.AddHeader("Authorization", cloud.AccessToken);
            return client.Execute(request).Content;
        }

        public string GetStartupLog(Application application, int instanceNumber, Cloud cloud)
        {
            //Get /apps/sroytest1/instances/0/files/logs/startup.log
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = "/apps/" + application.Name + "/" + instanceNumber + "/files/logs/startup.log";
            request.AddHeader("Authorization", cloud.AccessToken);
            return client.Execute(request).Content;
        }

        public void GetFiles(Application application, int instanceNumber, Cloud cloud)
        {
            
        }

        public IEnumerable<StatInfo> GetStats(Application argApplication, Cloud argCloud)
        {
            SortedDictionary<int, StatInfo> tmp = null;

            try
            {
                RestClient client = buildClient(argCloud);
                RestRequest request = buildRequest(Method.GET, Constants.APPS_PATH, argApplication.Name, "stats");
                RestResponse response = executeRequest(client, request);
                tmp = JsonConvert.DeserializeObject<SortedDictionary<int, StatInfo>>(response.Content);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var rv = new List<StatInfo>();
            foreach (KeyValuePair<int, StatInfo> kvp in tmp)
            {
                StatInfo si = kvp.Value;
                si.ID = kvp.Key;
                rv.Add(si);
            }
            return rv.ToArrayOrNull();
        }

        public IEnumerable<ExternalInstance> GetInstances(Application argApplication, Cloud argCloud) 
        {
            RestClient client = buildClient(argCloud);
            RestRequest request = buildRequest(Method.GET, Constants.APPS_PATH, argApplication.Name, "instances");
            var instances = executeRequest<Dictionary<string, ExternalInstance>>(client, request);
            return instances.Values.ToArrayOrNull();
        }
    }
}
