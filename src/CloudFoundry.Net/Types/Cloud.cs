using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.Types
{
    public class Cloud
    {
        public bool Connected { get; set; }
        public string AccessToken { get; set; }
        public string Email { get; set; }
        public string HostName { get; set; }
        public string Password { get; set; }
        public string ServerName { get; set; }
        public int TimeoutStart { get; set; }
        public int timeoutStop { get; set; }
        public string Url { get; set; }
    }
}
