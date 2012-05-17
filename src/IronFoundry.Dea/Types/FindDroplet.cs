namespace IronFoundry.Dea.Types
{
    using System;
    using Newtonsoft.Json;

    public class FindDroplet : Message
    {
        [JsonProperty(PropertyName = "droplet")]
        public uint DropletID { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "instances")]
        public Guid[] InstanceIds { get; set; }

        [JsonProperty(PropertyName = "indices")]
        public uint[] Indices { get; set; }

        [JsonProperty(PropertyName = "states")]
        public string[] States { get; set; }

        [JsonProperty("include_stats")]
        public bool IncludeStats { get; set; }
    }
}