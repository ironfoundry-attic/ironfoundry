namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using CloudFoundry.Net.Types;
    using Newtonsoft.Json;
    using RestSharp;

    internal class InfoHelper : BaseVmcHelper
    {
        public InfoHelper(VcapCredentialManager argCredentialManager)
            : base(argCredentialManager) { }

        public string GetLogs(Application argApp, ushort argInstance)
        {
            string logoutput = "";

            logoutput = "====stderr.log====\n";
            logoutput = logoutput + GetStdErrLog(argApp, argInstance);
            logoutput = logoutput + "\n====stdout.log====\n";
            logoutput = logoutput + GetStdOutLog(argApp, argInstance);
            logoutput = logoutput + "\n====startup.log====\n";
            logoutput = logoutput + GetStartupLog(argApp, argInstance);

            return logoutput;
        }

        public string GetStdErrLog(Application argApp, ushort argInstance)
        {
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.GET, Constants.APPS_PATH, argApp.Name, argInstance, "files/logs/stderr.log");
            return client.Execute(request).Content;
        }

        public string GetStdOutLog(Application argApp, ushort argInstance)
        {
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.GET, Constants.APPS_PATH, argApp.Name, argInstance, "files/logs/stdout.log");
            return client.Execute(request).Content;
        }

        public string GetStartupLog(Application argApp, ushort argInstance)
        {
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.GET, Constants.APPS_PATH, argApp.Name, argInstance, "files/logs/startup.log");
            return client.Execute(request).Content;
        }

        public void GetFiles(Application argApp, ushort argInstance)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StatInfo> GetStats(Application argApp)
        {
            SortedDictionary<int, StatInfo> tmp = null;

            try
            {
                RestClient client = buildClient();
                RestRequest request = buildRequest(Method.GET, Constants.APPS_PATH, argApp.Name, "stats");
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

        public IEnumerable<ExternalInstance> GetInstances(Application argApp)
        {
            RestClient client = buildClient();
            RestRequest request = buildRequest(Method.GET, Constants.APPS_PATH, argApp.Name, "instances");
            var instances = executeRequest<Dictionary<string, ExternalInstance>>(client, request);
            return instances.Values.ToArrayOrNull();
        }
    }
}