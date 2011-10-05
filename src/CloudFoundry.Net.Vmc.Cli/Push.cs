namespace CloudFoundry.Net.Vmc.Cli
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    static partial class Program
    {
        static bool push(IList<string> unparsed)
        {
            // TODO match ruby argument parsing
            if (unparsed.Count != 3)
            {
                Console.Error.WriteLine("Usage: vmc push appname path url"); // TODO usage statement standardization
                return false;
            }

            string appname = unparsed[0];
            string path    = unparsed[1];
            string url     = unparsed[2];

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

            var vc = new VcapClient();
            VcapClientResult rv = vc.Push(appname, url, di, 64);
            if (false == rv.Success)
            {
                Console.Error.WriteLine(rv.Message);
            }
            return rv.Success;
        }

    }
}