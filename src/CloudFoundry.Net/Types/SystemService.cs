using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Types
{
    public class SystemService : JsonBase
    {
        Datastore DataStoreService { get; set; }

        public SystemService() 
        {
            DataStoreService = new Datastore();
        }
    }

   internal class Datastore : JsonBase
    {
       [JsonProperty(PropertyName = "type")]
        string Type { get; set; } //Types supported are key/value, generic, database... could potentially be a static class or enum
       
       [JsonProperty(PropertyName = "version")]
        string Version { get; set; }

       [JsonProperty(PropertyName = "id")]
        int Id { get; set; }

       [JsonProperty(PropertyName = "vendor")]
        string Vendor { get; set; }

       [JsonProperty(PropertyName = "tiers")]
        Tiers Tiers { get; set; }

       [JsonProperty(PropertyName = "description")]
        string Description { get; set; } 

        public Datastore () 
        {
            Tiers = new Tiers();
        }
    }

    internal class Tiers : JsonBase
    {
        [JsonProperty(PropertyName = "free")]
        Type Type { get; set; } //Currently on showing Free but potentially other options in the future

        [JsonProperty(PropertyName = "order")]
        int Order { get; set; }
        
        public Tiers () {
            Type = new Type();
        }
    }

    internal class Type : JsonBase
    {
        [JsonProperty(PropertyName = "options")]
        Options Options { get; set; }

        public Type () {
            Options = new Options();
        }
    }
    internal class Options : JsonBase
    {

    }

    

}
