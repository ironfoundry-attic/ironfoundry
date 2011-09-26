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

        public List<Stats> GetStats(Application application, Cloud cloud)
        {
            var client = new RestClient();
            client.BaseUrl = cloud.Url;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = "/apps/" + application.Name + "/stats";
            request.AddHeader("Authorization", cloud.AccessToken);
            return (List<Stats>)JsonConvert.DeserializeObject(client.Execute(request).Content, typeof(List<Stats>));

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
            return (List<Instance>)JsonConvert.DeserializeObject(client.Execute(request).Content, typeof(List<Instance>));
        }

    }
}
