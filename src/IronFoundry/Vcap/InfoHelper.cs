namespace IronFoundry.Vcap
{
    using System;
    using System.Collections.Generic;
    using IronFoundry.Types;
    using Newtonsoft.Json;
    using RestSharp;

    internal class InfoHelper : BaseVmcHelper
    {
        public InfoHelper(VcapCredentialManager credMgr) : base(credMgr) { }

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
            var r = new VcapRequest(credMgr, Constants.APPS_PATH, argApp.Name, argInstance, "files/logs/stderr.log");
            return r.Execute().Content;
        }

        public string GetStdOutLog(Application argApp, ushort argInstance)
        {
            var r = new VcapRequest(credMgr, Constants.APPS_PATH, argApp.Name, argInstance, "files/logs/stdout.log");
            return r.Execute().Content;
        }

        public string GetStartupLog(Application argApp, ushort argInstance)
        {
            var r = new VcapRequest(credMgr, Constants.APPS_PATH, argApp.Name, argInstance, "files/logs/startup.log");
            return r.Execute().Content;
        }

        public void GetFiles(Application argApp, ushort argInstance)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StatInfo> GetStats(Application argApp)
        {
            var r = new VcapRequest(credMgr, Constants.APPS_PATH, argApp.Name, "stats");
            RestResponse response = r.Execute();
            var tmp = JsonConvert.DeserializeObject<SortedDictionary<int, StatInfo>>(response.Content);

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
            var r = new VcapRequest(credMgr, Constants.APPS_PATH, argApp.Name, "instances");
            var instances = r.Execute<Dictionary<string, ExternalInstance>>();
            return instances.Values.ToArrayOrNull();
        }
    }
}