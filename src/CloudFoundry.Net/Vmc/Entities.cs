using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.Vmc
{
    internal class VmcApplication
    {
        public string name { get; set; }
        public string[] uris { get; set; }
        public int instances { get; set; }
        public resources resources { get; set; }
        public staging staging{ get; set;}
    }

    internal class staging 
    {
        public string framework { get; set; }
        public string runtime { get; set; }
    }

    internal class resources 
    {
        public int memory { get; set; }
    }

    internal class Resource
    {
        public long size;
        public string sha1;
        public string fn;
    }
}
