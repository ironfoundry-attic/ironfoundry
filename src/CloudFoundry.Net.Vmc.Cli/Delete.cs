namespace CloudFoundry.Net.Vmc.Cli
{
    using System;
    using System.Collections.Generic;

    static partial class Program
    {
        const string deleteFmt = "Deleting application [{0}]: ";

        static bool Delete(IList<string> unparsed)
        {
            // TODO match ruby argument parsing
            if (unparsed.Count != 1)
            {
                Console.Error.WriteLine("Not enough arguments for [delete]");
                Console.Error.WriteLine("Usage: vmc delete <appname>"); // TODO usage statement standardization
                return false;
            }

            string appname = unparsed[0];

            Console.Write(deleteFmt, appname);

            IVcapClient vc = new VcapClient();
            vc.Delete(appname);
            return true;
        }
    }
}