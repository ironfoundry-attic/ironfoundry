namespace IronFoundry.Dea.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Types;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using IronFoundry.Dea.Properties;

    public class VarzProvider : IVarzProvider
    {
        // vcap discover keys
        const string typeKey        = "type";
        const string indexKey       = "index";
        const string uuidKey        = "uuid";
        const string hostKey        = "host";
        const string credentialsKey = "credentials";
        const string startKey       = "start";

        private readonly string[] varzDiscoverKeys = new[]
        {
            typeKey, indexKey, uuidKey, hostKey, credentialsKey, startKey
        };

        const string appsMaxMemoryKey      = "apps_max_memory";
        const string appsReservedMemoryKey = "apps_reserved_memory";
        const string appsUsedMemoryKey     = "apps_used_memory";
        const string numAppsKey            = "num_apps";
        const string stateKey              = "state";
        const string runtimesKey           = "runtimes";
        const string frameworksKey         = "frameworks";
        const string runningAppsKey        = "running_apps";
        const string numCoresKey           = "num_cores";

        private readonly ReaderWriterLockSlim varzLock = new ReaderWriterLockSlim();
        private readonly IDictionary<string, object> varz = new Dictionary<string, object>();
        private readonly ILog log;

        public VarzProvider(ILog log)
        {
            this.log = log;

            setVarzValue(numCoresKey, Environment.ProcessorCount);
        }

        public string GetVarzJson()
        {
            string rv = String.Empty;

            try
            {
                varzLock.ExitWriteLock();
                rv = JsonConvert.SerializeObject(varz);
            }
            finally
            {
                varzLock.ExitReadLock();
            }

            log.Debug(Resources.VarzProvider_GetVarzJson_Fmt, rv);
            return rv;
        }

        public VcapComponentDiscover Discover
        {
            set
            {
                JObject jobj = JObject.FromObject(value);
                foreach (string key in varzDiscoverKeys)
                {
                    JToken tmp;
                    if (jobj.TryGetValue(key, out tmp))
                    {
                        setVarzValue(key, tmp);
                    }
                }
            }
        }

        public ulong MaxMemoryMB
        {
            set { setVarzValue(appsMaxMemoryKey, value); }
        }

        public ulong MemoryReservedMB
        {
            set { setVarzValue(appsReservedMemoryKey, value); }
        }

        public ulong MemoryUsedMB
        {
            set { setVarzValue(appsUsedMemoryKey, value); }
        }

        public uint MaxClients
        {
            set { setVarzValue(numAppsKey, value); }
        }

        public string State
        {
            set { setVarzValue(stateKey, value); }
        }

        private void setVarzValue(string key, object value)
        {
            try
            {
                varzLock.EnterWriteLock();
                varz[key] = value;
            }
            finally
            {
                varzLock.ExitWriteLock();
            }
        }


        public IEnumerable<string> RunningAppsJson
        {
            set { setVarzValue(runningAppsKey, value); }
        }

        public IDictionary<string, Metric> RuntimeMetrics
        {
            set { setVarzValue(runtimesKey, JsonConvert.SerializeObject(value)); }
        }

        public IDictionary<string, Metric> FrameworkMetrics
        {
            set { setVarzValue(frameworksKey, JsonConvert.SerializeObject(value)); }
        }
    }
}