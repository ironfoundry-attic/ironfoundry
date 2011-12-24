namespace IronFoundry.Vmc
{
    using System;
    using System.Collections.Generic;
    using IronFoundry.Vcap;

    static partial class Program
    {
        const string deleteFmt = "Deleting application [{0}]: ";

        static bool Delete(IList<string> unparsed)
        {
            if (unparsed.Count != 1)
            {
                Console.Error.WriteLine("Not enough arguments for [delete]");
                Console.Error.WriteLine("Usage: vmc delete <appname>");
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
