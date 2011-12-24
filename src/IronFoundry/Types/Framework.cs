namespace IronFoundry.Types
{
    using Newtonsoft.Json;

    public class Framework :EntityBase
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "runtimes")]
        public Runtime[] Runtimes { get; private set; }

        [JsonProperty(PropertyName="appservers")]
        public AppServer[] AppServers { get; private set; }
    }

    public class Runtime : EntityBase
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; private set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; private set; }
    }

    public class AppServer : EntityBase
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; private set; }
    }
}