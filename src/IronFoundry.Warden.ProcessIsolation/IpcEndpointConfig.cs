using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronFoundry.Warden.ProcessIsolation
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /* note: these two endpoints should end up being the same thing, for the ProcessHostService,
     * it assumes it runs as the user, hence using Environment.UserName for the pipe path.
     */
    public static class IpcEndpointConfig
    {
        public static EndpointAddress ClientAddress(string uniquePipeID)
        {
            return new EndpointAddress(String.Format("net.pipe://localhost/{0}/IProcessHostService", uniquePipeID));
        }

        public static string ServiceAddress(string uniquePipeID = null)
        {
            return String.Format("net.pipe://localhost/{0}/IProcessHostService", String.IsNullOrWhiteSpace(uniquePipeID) ? Environment.UserName : uniquePipeID);
        }

        public static Binding Binding()
        {
            return new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
        }
    }
}
