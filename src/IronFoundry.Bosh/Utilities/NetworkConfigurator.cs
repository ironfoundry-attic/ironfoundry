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
        private readonly IPAddress ip;
        private readonly IPAddress netmask;
        private readonly IPAddress gateway;
        private readonly List<IPAddress> dnsAddrs = new List<IPAddress>();

        private const string NetshFmt = @"interface ipv4 set address name=""{0}"" source=static address={1} mask={2} gateway={3}";
        private const string NetshFirstDnsFmt = @"interface ipv4 set dns name=""{0}"" source=static addr={1}";
        private const string NetshRestDnsFmt = @"interface ipv4 add dns name=""{0}"" addr={1}";

        public NetworkConfigurator(ILog log, string ip, string netmask, string gateway, IEnumerable<string> dns)
        {
            this.log = log;

            try
            {
                this.ip = IPAddress.Parse(ip);
                this.netmask = IPAddress.Parse(netmask);
                this.gateway = IPAddress.Parse(gateway);
                if (false == dns.IsNullOrEmpty())
                {
                    dnsAddrs.AddRange(dns.Select(d => IPAddress.Parse(d)));
                }
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid IP argument!", ex);
            }
        }

        public bool ConfigureNetwork()
        {
            var ms = new ManagementScope(@"\root\cimv2");
            var query = new SelectQuery("Win32_NetworkAdapter",
                "AdapterTypeId = '0' AND Availability = '3' AND ConfigManagerErrorCode = '0' AND NetConnectionStatus = '2' AND NetEnabled = 'True'");
            var searcher = new ManagementObjectSearcher(ms, query);
            var managementObject = searcher.Get().Cast<ManagementObject>().First();
            string netConnectionID = (string)managementObject["NetConnectionID"];

            string netshArgs = String.Format(NetshFmt, netConnectionID, ip.ToString(), netmask.ToString(), gateway.ToString());

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
                        netshArgs = String.Format(NetshFirstDnsFmt, netConnectionID, dnsAddress.ToString());
                        firstDns = false;
                    }
                    else
                    {
                        netshArgs = String.Format(NetshRestDnsFmt, netConnectionID, dnsAddress.ToString());
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

            return success;
        }
    }
}