using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.Vmc
{
    internal class VmcBase
    {
        const string USER_PATH = "/users";
        public string username { get; set; }
        public string password { get; set; }
        public string apiUrl { get; set; }
        public string appName { get; set; }
    }
}
