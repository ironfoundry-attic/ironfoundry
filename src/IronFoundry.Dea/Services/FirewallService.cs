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
        const string INetFwPolicy2ProgID = "HNetCfg.FwPolicy2";
        const string INetFwRuleProgID = "HNetCfg.FWRule";

        public void Open(ushort port, string name)
        {
            INetFwRule2 firewallRule = getComObject<INetFwRule2>(INetFwRuleProgID);
            firewallRule.Description = name;
            firewallRule.Name = name;
            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
            firewallRule.LocalPorts = port.ToString();

            INetFwPolicy2 firewallPolicy = getComObject<INetFwPolicy2>(INetFwPolicy2ProgID);
            firewallPolicy.Rules.Add(firewallRule);
        }

        public void Close(string name)
        {
            INetFwPolicy2 firewallPolicy = getComObject<INetFwPolicy2>(INetFwPolicy2ProgID);
            firewallPolicy.Rules.Remove(name);
        }

        private static T getComObject<T>(string progID)
        {
            Type t = Type.GetTypeFromProgID(progID, true);
            return (T)Activator.CreateInstance(t);
        }
    }
}