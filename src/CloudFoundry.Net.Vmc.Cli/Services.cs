namespace CloudFoundry.Net.Vmc.Cli
{
    using System.Collections.Generic;
    using CloudFoundry.Net.Types;

    static partial class Program
    {
        static bool services(IList<string> unparsed)
        {
            var vc = new VcapClient();
            return true;
        }
    }
}