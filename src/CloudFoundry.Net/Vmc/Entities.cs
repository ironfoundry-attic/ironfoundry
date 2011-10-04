namespace CloudFoundry.Net.Vmc
{
    using Newtonsoft.Json;

    public class AppManifest
    {
        [JsonProperty(PropertyName="name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName="uris")]
        public string[] Uris { get; set; }

        [JsonProperty(PropertyName="instances")]
        public ushort Instances { get; set; }

        [JsonProperty(PropertyName="resources")]
        public Resources Resources { get; set; }

        [JsonProperty(PropertyName="staging")]
        public Staging Staging { get; set;}
    }

    public class Staging 
    {
        [JsonProperty(PropertyName="framework")]
        public string Framework { get; set; }

        [JsonProperty(PropertyName="runtime")]
        public string Runtime { get; set; }
    }

    public class Resources 
    {
        [JsonProperty(PropertyName="memory")]
        public uint Memory { get; set; }
    }

    public class Resource
    {
        [JsonProperty(PropertyName="size")]
        public ulong Size { get; set; }

        [JsonProperty(PropertyName="sha1")]
        public string SHA1 { get; set; }

        [JsonProperty(PropertyName="fn")]
        public string FN { get; set; }
    }
}