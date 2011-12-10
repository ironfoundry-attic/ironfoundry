namespace CloudFoundry.Net.Vmc.Cli
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    static partial class Program
    {
        static bool Push(IList<string> unparsed)
        {
            // TODO match ruby argument parsing
            if (unparsed.Count < 3 || unparsed.Count > 4)
            {
                Console.Error.WriteLine("Usage: vmc push <appname> <path> <url> [service] --instances N --mem MB"); // TODO usage statement standardization
                return false;
            }

            string appname = unparsed[0];
            string path    = unparsed[1];
            string fqdn    = unparsed[2];

            string[] serviceNames = null;
            if (unparsed.Count == 4)
            {
                serviceNames = new[] { unparsed[3] };
            }

            DirectoryInfo di = null;
            if (Directory.Exists(path))
            {
                di = new DirectoryInfo(path);
            }
            else
            {
                Console.Error.WriteLine(String.Format("Directory '{0}' does not exist."));
                return false;
            }

            IVcapClient vc = new VcapClient();
            VcapClientResult rv = vc.Push(appname, fqdn, instances, di, memoryMB, serviceNames);
            if (false == rv.Success)
            {
                Console.Error.WriteLine(rv.Message);
            }
            return rv.Success;
        }
    }
}