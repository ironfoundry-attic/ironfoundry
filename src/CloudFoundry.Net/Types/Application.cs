using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Types
{
    public class Application : JsonBase
    {
        
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "staging")]
        public Staging Staging { get; set; }

        [JsonProperty(PropertyName = "uris")]
        public string[] Uris { get; set; }

        [JsonProperty(PropertyName = "instances")]
        public int Instances { get; set; }

        [JsonProperty(PropertyName = "runningInstances")]
        public int RunningInstances { get; set; }

        [JsonProperty(PropertyName = "resources")]
        public Resources Resources { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "services")]
        public string[] Services { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "env")]
        public Env Enviorment { get; set; }

        [JsonProperty(PropertyName = "meta")]
        public AppMeta MetaData { get; set; }

        public Application() {
            Staging = new Types.Staging();
            Resources = new Resources();
            MetaData = new AppMeta();
        }
    }

    public class Staging : JsonBase 
    {
        [JsonProperty(PropertyName = "model")]
        string Model { get; set; }

        [JsonProperty(PropertyName = "stack")]
        string Stack { get; set; }
    }

    public class Resources : JsonBase
    {
        [JsonProperty(PropertyName = "memory")]
        int Memory { get; set; }

        [JsonProperty(PropertyName = "disk")]
        int Disk { get; set; }

        [JsonProperty(PropertyName = "fds")]
        int Fds { get; set; }

        

    }

   public class Env
    {
        public Env () {

        }
    }

   public class AppMeta : JsonBase
    {
        [JsonProperty(PropertyName = "version")]
        int Version { get; set; }

        [JsonProperty(PropertyName = "created")]
        int Created { get; set; }

    }
}
