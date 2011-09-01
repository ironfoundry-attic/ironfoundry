using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudFoundry.Net.Vmc
{
    public class VmcManager : IVmcClient
    {
        public string URL { get; set; }
        public string AccessToken { get; set; }

        public string LogIn(string email, string password)
        {
            VmcAdministration cfa = new VmcAdministration();
            return cfa.Login(email, password, URL);
        }

        public string LogOut(string authenticationtoken)
        {
            throw new NotImplementedException();
        }

        public string Push(string appname, string fdqn, string fileURI, string framework, string memorysize)
        {
            VmcApps cfapps = new VmcApps();
            var app =  cfapps.PushApp(appname, URL, AccessToken, fileURI, fdqn, framework, null,memorysize, null);
            return app;
        }

        public string StartApp(string appname)
        {
            throw new NotImplementedException();
        }

        public string StopApp(string appname)
        {
            throw new NotImplementedException();
        }

        public string RestartApp(string appname)
        {
            throw new NotImplementedException();
        }

        public string Info()
        {
            VmcInit init = new VmcInit();
            return init.Info(AccessToken,URL);
        }
    }
}
