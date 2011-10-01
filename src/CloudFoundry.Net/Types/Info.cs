using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Types
{
    public class Info : Message
    {
        [JsonProperty(PropertyName = "name")]
        string Name { get; set; }

        [JsonProperty(PropertyName = "build")]
        string Build { get; set; }

        [JsonProperty(PropertyName = "support")]
        string Support { get; set; }

        [JsonProperty(PropertyName = "version")]
        string Version { get; set; }

        [JsonProperty(PropertyName = "description")]
        string Description { get; set; }

        [JsonProperty(PropertyName = "user")]
        string User { get; set; }

        [JsonProperty(PropertyName = "limits")]
        ResourceLimits Limits { get; set; }

        [JsonProperty(PropertyName = "useage")]
        ResourceUsage Usage { get; set; }

        [JsonProperty(PropertyName = "framework")]
        Frameworks frameworks { get; set; }

        Info() {
            Limits = new ResourceLimits();
            Usage = new ResourceUsage();
        }
    }
    internal class ResourceLimits : EntityBase
    {

        [JsonProperty(PropertyName = "memory")]
        int Memory { get; set; }

        [JsonProperty(PropertyName = "app_uris")]
        int AppURIs { get; set; }

        [JsonProperty(PropertyName = "services")]
        int Services { get; set; }

        [JsonProperty(PropertyName = "apps")]
        int Apps { get; set; }


    }

     internal class ResourceUsage : EntityBase
    {
        
        [JsonProperty(PropertyName = "memory")]
        int Memory { get; set; }

        [JsonProperty(PropertyName = "apps")]
        int Apps { get; set; }

        [JsonProperty(PropertyName = "services")]
        int Services { get; set; }

    }
}
