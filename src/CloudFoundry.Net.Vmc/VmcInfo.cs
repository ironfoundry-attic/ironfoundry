using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Types;
using RestSharp;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Vmc
{
    internal class VmcInfo
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

        public List<StatInfo> GetStats(Application application, Cloud cloud)
        {
            var list = new List<StatInfo>();
            try
            {
                var client = new RestClient();
                client.BaseUrl = cloud.Url;
                var request = new RestRequest();
                request.Method = Method.GET;
                request.Resource = "/apps/" + application.Name + "/stats";
                request.AddHeader("Authorization", cloud.AccessToken);
                var response = client.Execute(request).Content;
                list = JsonConvert.DeserializeObject<List<StatInfo>>(response);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }

        public List<Instance> GetInstances(Application application, Cloud cloud) 
        {
            //GET /apps/sroytest1/instances
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = "/apps/" + application.Name + "/instances";
            request.AddHeader("Authorization", cloud.AccessToken);
            var response = client.Execute(request).Content;
            var list = JsonConvert.DeserializeObject<List<Instance>>(response);
            return list;
        }

    }
}
