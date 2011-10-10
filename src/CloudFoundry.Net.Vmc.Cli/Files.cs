namespace CloudFoundry.Net.Vmc.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    static partial class Program
    {
        static bool Files(IList<string> unparsed)
        {
            if (unparsed.Count != 2)
            {
                Console.Error.WriteLine("Too many arguments for [files]: {0}", String.Join(", ", unparsed.Select(s => String.Format("'{0}'", s))));
                Console.Error.WriteLine("Usage: vmc files <appname> <path>"); // TODO usage statement standardization
                return false;
            }

            string appname = unparsed[0];
            string path = unparsed[1];

            IVcapClient vc = new VcapClient();
            Console.Write(vc.Files(appname, path, 0));
            return true;
        }
    }
}