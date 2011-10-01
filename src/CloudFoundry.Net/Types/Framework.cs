using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Types
{
    class Framework :EntityBase
    {
        [JsonProperty(PropertyName = "name")]
        string Name { get; set; }

        [JsonProperty(PropertyName = "runtimes")]
        Runtimes FrameworkRuntimes { get; set; }

        [JsonProperty(PropertyName="appservers")]
        AppServers FrameworkAppServers { get; set; }

        [JsonProperty(PropertyName = "detection")]
        Detection FrameworkDetection { get; set; }

        public Framework() {
            FrameworkRuntimes = new Runtimes();
            FrameworkAppServers = new AppServers();
            FrameworkDetection = new Detection();
        }
    }
    internal class Runtimes : EntityBase
    {
        [JsonProperty(PropertyName = "version")]
        string Version { get; set; }

        [JsonProperty(PropertyName = "name")]
        string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        string Description { get; set; }
    }
    internal class AppServers : EntityBase
    {
        [JsonProperty(PropertyName = "name")]
        string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        string Description { get; set; }
    }
    internal class Detection : EntityBase
    {
        
        string FileExtension { get; set; }
        string InternalPattern { get; set; }
        bool Enabeled { get; set; }
    }

}
