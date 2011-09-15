using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        string type { get; set; } //Types supported are key/value, generic, database... could potentially be a static class
        string Version { get; set; }
        int Id { get; set; }
        string Vendor { get; set; }
        Tiers Tiers { get; set; }
        string Description { get; set; } 

        public Datastore () 
        {
            Tiers = new Tiers();
        }
    }

    internal class Tiers : JsonBase
    {
        string type { get; set; } //Currently on showing Free but potentially other options in the future
        int order { get; set; }
        Options options { get; set; }
    }

    internal class Options : JsonBase
    {

    }

    

}
