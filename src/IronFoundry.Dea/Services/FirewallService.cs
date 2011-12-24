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
        // TODO: Enable rules in all profiles
        public void Open(ushort port, string name)
        {
            INetFwOpenPort openPort = getComObject<INetFwOpenPort>("HNetCfg.FWOpenPort");
            openPort.Port = port;
            openPort.Name = name;
            openPort.Enabled = true;
            openPort.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;
            openPort.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;

            INetFwMgr fwMgr = getComObject<INetFwMgr>("HNetCfg.FwMgr");
            INetFwOpenPorts openPorts = (INetFwOpenPorts)fwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;
            openPorts.Add(openPort);
        }

        public void Close(ushort port)
        {
            INetFwMgr fwMgr = getComObject<INetFwMgr>("HNetCfg.FwMgr");
            INetFwOpenPorts openPorts = (INetFwOpenPorts)fwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;
            openPorts.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
        }

        private static T getComObject<T>(string progID)
        {
            Type t = Type.GetTypeFromProgID(progID, true);
            return (T)Activator.CreateInstance(t);
        }
    }
}