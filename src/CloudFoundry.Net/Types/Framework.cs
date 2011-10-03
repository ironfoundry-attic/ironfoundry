namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public class Framework :EntityBase
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "runtimes")]
        public Runtimes FrameworkRuntimes { get; set; }

        [JsonProperty(PropertyName="appservers")]
        public AppServers FrameworkAppServers { get; set; }

        [JsonProperty(PropertyName = "detection")]
        public Detection FrameworkDetection { get; set; }
    }

    public class Runtimes : EntityBase
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }

    public class AppServers : EntityBase
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }

    public class Detection : EntityBase
    {
        public string FileExtension { get; set; }
        public string InternalPattern { get; set; }
        public bool Enabeled { get; set; }
    }
}