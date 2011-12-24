namespace IronFoundry.Dea.Services
{
    using System;
    using NetFwTypeLib;

    /// <summary>
    /// http://www.shafqatahmed.com/2008/01/controlling-win.html
    /// http://blogs.msdn.com/b/securitytools/archive/2009/08/21/automating-windows-firewall-settings-with-c.aspx
    /// </summary>
    public class FirewallService : IFirewallService
    {
        private readonly bool firewallEnabled;

        public FirewallService()
        {
            INetFwMgr fwMgr = getFirewallManager();
            this.firewallEnabled = fwMgr.LocalPolicy.CurrentProfile.FirewallEnabled;
        }

        public void Open(ushort port, string name)
        {
            if (firewallEnabled)
            {
                Type netFwOpenPortType = Type.GetTypeFromProgID("HNetCfg.FWOpenPort");
                INetFwOpenPort openPort = (INetFwOpenPort)Activator.CreateInstance(netFwOpenPortType);
                openPort.Port = port;
                openPort.Name = name;
                openPort.Enabled = true;
                openPort.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;

                INetFwMgr fwMgr = getFirewallManager();
                INetFwOpenPorts openPorts = (INetFwOpenPorts)fwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;
                openPorts.Add(openPort);
            }
        }

        public void Close(ushort port)
        {
            if (firewallEnabled)
            {
                INetFwMgr fwMgr = getFirewallManager();
                INetFwOpenPorts openPorts = (INetFwOpenPorts)fwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;
                openPorts.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
            }
        }

        private static INetFwMgr getFirewallManager()
        {
            Type firewallManagerType = Type.GetTypeFromProgID("HNetCfg.FwMgr");
            return (INetFwMgr)Activator.CreateInstance(firewallManagerType);
        }
    }
}