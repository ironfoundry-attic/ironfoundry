using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Types
{
    class Application : JsonBase
    {
        [JsonProperty(PropertyName = "name")]
        string Name { get; set; }

        [JsonProperty(PropertyName = "staging")]
        Staging Staging { get; set; }

        [JsonProperty(PropertyName = "uris")]
        string[] Uris { get; set; }

        [JsonProperty(PropertyName = "instances")]
        int Instances { get; set; }

        [JsonProperty(PropertyName = "runningInstances")]
        int RunningInstances { get; set; }

        [JsonProperty(PropertyName = "resources")]
        Resources Resources { get; set; }

        [JsonProperty(PropertyName = "state")]
        string State { get; set; }

        [JsonProperty(PropertyName = "services")]
        string[] Services { get; set; }

        [JsonProperty(PropertyName = "version")]
        string Version { get; set; }

        [JsonProperty(PropertyName = "env")]
        Env Enviorment { get; set; }

        [JsonProperty(PropertyName = "meta")]
        AppMeta MetaData { get; set; }

        public Application() {
            Staging = new Types.Staging();
            Resources = new Resources();
            MetaData = new AppMeta();
        }
    }

    internal class Staging : JsonBase 
    {
        [JsonProperty(PropertyName = "model")]
        string Model { get; set; }

        [JsonProperty(PropertyName = "stack")]
        string Stack { get; set; }
    }

    internal class Resources : JsonBase
    {
        [JsonProperty(PropertyName = "memory")]
        int Memory { get; set; }

        [JsonProperty(PropertyName = "disk")]
        int Disk { get; set; }

        [JsonProperty(PropertyName = "fds")]
        int Fds { get; set; }

        

    }

    internal class Env
    {
        public Env () {

        }
    }

    internal class AppMeta : JsonBase
    {
        [JsonProperty(PropertyName = "version")]
        int Version { get; set; }

        [JsonProperty(PropertyName = "created")]
        int Created { get; set; }

    }
}
