namespace IronFoundry.Dea.Types
{
    using Newtonsoft.Json;

    public class Droplet : EntityBase
    {
        [JsonProperty(PropertyName = "droplet")]
        public uint ID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "uris")]
        public string[] Uris { get; set; }

        [JsonProperty(PropertyName = "runtime")]
        public string Runtime { get; set; }

        [JsonProperty(PropertyName = "framework")]
        public string Framework { get; set; }

        [JsonProperty(PropertyName = "sha1")]
        public string Sha1 { get; set; }

        [JsonProperty(PropertyName = "executableFile")]
        public string ExecutableFile { get; set; }

        [JsonProperty(PropertyName = "executableUri")]
        public string ExecutableUri { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "services")]
        public Service[] Services { get; set; }

        [JsonProperty(PropertyName = "limits")]
        public Limits Limits { get; set; }

        [JsonProperty(PropertyName = "env")]
        public string[] Env { get; set; }

        [JsonProperty(PropertyName = "users")]
        public string[] Users { get; set; }

        [JsonProperty(PropertyName = "index")]
        public uint InstanceIndex { get; set; }

        [JsonIgnore]
        public bool FrameworkSupported
        {
            get { return Constants.IsSupportedFramework(Framework); }
        }

        [JsonIgnore]
        public bool IsAspNet
        {
            get { return Constants.IsAspNet(Framework); }
        }
    }
}