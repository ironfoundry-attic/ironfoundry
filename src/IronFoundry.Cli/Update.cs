namespace IronFoundry.Cli
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using IronFoundry.Vcap;

    static partial class Program
    {
        static bool Update(IList<string> unparsed)
        {
            // TODO match ruby argument parsing
            if (unparsed.Count != 2)
            {
                Console.Error.WriteLine("Usage: vmc update <appname> <path>"); // TODO usage statement standardization
                return false;
            }

            string appname = unparsed[0];
            string path    = unparsed[1];

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
            VcapClientResult rv = vc.Update(appname, di);
            if (false == rv.Success)
            {
                Console.Error.WriteLine(rv.Message);
            }
            return rv.Success;
        }
    }
}