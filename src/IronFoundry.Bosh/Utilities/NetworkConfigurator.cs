namespace IronFoundry.Bosh.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management;
    using System.Net;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.Utilities;

    /*
     * TODO:
     * set ip address
       netsh interface ipv4 set address name="Local Area Connection" source=static address=%1 mask=%2 gateway=%3
       netsh interface ipv4 set dns name="Local Area Connection" source=static addr=%4
       netsh interface ipv4 add dns name="Local Area Connection" addr=%5
     */
    public class NetworkConfigurator
    {
        private static readonly TimeSpan NetworkConfiguratorWaitInterval = TimeSpan.FromSeconds(5);

        private readonly ILog log;

        private const string NetshFmt = @"interface ipv4 set address name=""{0}"" source=static address={1} mask={2} gateway={3}";
        private const string NetshFirstDnsFmt = @"interface ipv4 set dns name=""{0}"" source=static addr={1}";
        private const string NetshRestDnsFmt = @"interface ipv4 add dns name=""{0}"" addr={1}";

        public NetworkConfigurator(ILog log)
        {
            this.log = log;
            this.NetworkConfigured = GetNetworkConfigured();
        }

        public bool NetworkConfigured
        {
            get;
            private set;
        }

        /// <summary>
        /// NB: can block if error happens with "netsh" command.
        /// </summary>
        public bool ConfigureNetwork(string argIP, string argNetmask, string argGateway, IEnumerable<string> argDns)
        {
            IPAddress ip, netmask, gateway;
            var dnsAddrs = new List<IPAddress>();
            try
            {
                ip = IPAddress.Parse(argIP);
                netmask = IPAddress.Parse(argNetmask);
                gateway = IPAddress.Parse(argGateway);
                if (false == argDns.IsNullOrEmpty())
                {
                    dnsAddrs.AddRange(argDns.Select(d => IPAddress.Parse(d)));
                }
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid IP argument!", ex);
            }

            ConnectionInfo ci = GetFirstEthernetConnectionInfo();

            string netshArgs = String.Format(NetshFmt, ci.NetConnectionID, ip.ToString(), netmask.ToString(), gateway.ToString());

            bool success = false;
            var exec = new ExecCmd(log, "netsh", netshArgs);
            ExecCmdResult rslt = exec.Run(5, NetworkConfiguratorWaitInterval);
            success = rslt.Success;
            if (rslt.Success)
            {
                bool firstDns = true;
                foreach (var dnsAddress in dnsAddrs)
                {
                    if (firstDns)
                    {
                        netshArgs = String.Format(NetshFirstDnsFmt, ci.NetConnectionID, dnsAddress.ToString());
                        firstDns = false;
                    }
                    else
                    {
                        netshArgs = String.Format(NetshRestDnsFmt, ci.NetConnectionID, dnsAddress.ToString());
                    }
                    exec = new ExecCmd(log, "netsh", netshArgs);
                    rslt = exec.Run(5, NetworkConfiguratorWaitInterval);
                    success = rslt.Success;
                    if (false == success)
                    {
                        break;
                    }
                }
            }

            if (success)
            {
                this.NetworkConfigured = true;
            }

            return success;
        }

        private bool GetNetworkConfigured()
        {
            /*
             * Check Win32_NetworkAdapterConfiguration - DHCPEnabled or IPAddress 169.254.X.X means not configured!
             * http://packetlife.net/blog/2008/sep/24/169-254-0-0-addresses-explained/
             */
            ConnectionInfo ci = GetFirstEthernetConnectionInfo();

            ManagementObject mo = ci.ManagementObject;
            ManagementObject adapterConfiguration = mo.GetRelated("Win32_NetworkAdapterConfiguration").Cast<ManagementObject>().First();

            bool dhcpEnabled = (bool)adapterConfiguration["DHCPEnabled"];
            bool apipaAddressPresent = ((string[])adapterConfiguration["IPAddress"]).Any(ip => ip.StartsWith("169.254."));

            return false == dhcpEnabled && false == apipaAddressPresent;
        }

        private class ConnectionInfo
        {
            public string NetConnectionID { get; set; }
            public ManagementObject ManagementObject { get; set; }
        }

        private static ConnectionInfo GetFirstEthernetConnectionInfo()
        {
            var ms = new ManagementScope(@"\root\cimv2");
            var query = new SelectQuery("Win32_NetworkAdapter",
                "AdapterTypeId = '0' AND Availability = '3' AND ConfigManagerErrorCode = '0' AND NetConnectionStatus = '2' AND NetEnabled = 'True'");
            var searcher = new ManagementObjectSearcher(ms, query);
            var managementObject = searcher.Get().Cast<ManagementObject>().First();
            return new ConnectionInfo
                {
                    NetConnectionID = (string)managementObject["NetConnectionID"],
                    ManagementObject = managementObject,
                };
        }
    }
}