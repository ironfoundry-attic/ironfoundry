namespace CloudFoundry.Net.Vmc.Cli
{
    using System;
    using System.Collections.Generic;

    static partial class Program
    {
        static bool Apps(IList<string> unparsed)
        {
            // TODO match ruby argument parsing
            if (unparsed.Count != 0)
            {
                Console.Error.WriteLine("Too many arguments for [apps]");
                Console.Error.WriteLine("Usage: vmc apps"); // TODO usage statement standardization
                return false;
            }
            // TODO IVcapClient vc = new VcapClient();
            // vc.Delete(appname);
            return true;
        }
    }
}