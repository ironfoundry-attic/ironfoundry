namespace IronFoundry.Test
{
    using System;
    using NetFwTypeLib;
    using Xunit;

    public class FirewallTests
    {
        [Fact(Skip="Reqires Admin Privileges")]
        public void Create_Rule_In_All_Profiles()
        {
            string name = "IRONFOUNDRY TEST";

            INetFwRule2 firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            firewallRule.Name = name;
            firewallRule.Description = name;
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
            firewallRule.LocalPorts = "31337";

            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallPolicy.Rules.Add(firewallRule);

            firewallPolicy = null;

            firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            INetFwRule found = null;
            foreach (INetFwRule rule in firewallPolicy.Rules)
            {
                if (rule.Name == name)
                {
                    found = rule;
                }
            }
            Assert.NotNull(found);
            Assert.Equal("31337", found.LocalPorts);
        }
    }
}