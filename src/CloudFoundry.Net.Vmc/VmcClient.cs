using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.Vmc
{
    internal class VmcClient 
    {
        public bool Authenticate(string Username, string Password)
        {
            return true;
        }

        public string GetInfo(string authenticationtoken)
        {
            return null;
        }

        public bool DeployApplication(string fileURI)
        {
            return true;
        }

        public bool Logout(string authenticationtoken)
        {
            return true;
        }

        public string StartApp()
        {
            return null;
        }

        public string StopApp()
        {
            return null;
        }
        public string RestartApp()
        {
            return null;
        }
    }
}
