using System;
using IronFoundry.Dea.JsonConverters;
using Newtonsoft.Json;

namespace IronFoundry.Dea.Types
{
    public class Discover : EntityBase
    {
        [JsonProperty(PropertyName = "droplet"), JsonConverter(typeof (VcapGuidConverter))]
        public Guid DropletID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "runtime")]
        public string Runtime { get; set; }

        [JsonProperty(PropertyName = "sha")]
        public string Sha { get; set; }

        [JsonProperty(PropertyName = "limits")]
        public Limits Limits { get; set; }
    }
}