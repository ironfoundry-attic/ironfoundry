using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.Vmc
{
    public interface IVmcClient
    {
        string URL { get; set; }
        string AccessToken { get; set; }
        string LogIn (string email, string password);
        string Push(string appname, string fdqn, string fileURI, string framework, string memorysize);
        string StartApp(string appname);
        string StopApp(string appname);
        string RestartApp(string appname);
        string Info();
    }
}
