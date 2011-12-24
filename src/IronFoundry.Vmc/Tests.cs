#if DEBUG
namespace IronFoundry.Vmc
{
    using System;
    using System.Collections.Generic;
    using IronFoundry.Types;
    using IronFoundry.Vcap;
    using Newtonsoft.Json;

    static partial class Program
    {
        static bool TestFiles(IList<string> unparsed)
        {
            string appname = unparsed[0];
            string path = unparsed[1];
            IVcapClient vc = new VcapClient();
            VcapFilesResult result = vc.Files(appname, path, 0);
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            return true;
        }

        static bool TestStats(IList<string> unparsed)
        {
            string appname = unparsed[0];
            IVcapClient vc = new VcapClient();
            Application app = vc.GetApplication(appname);
            IEnumerable<StatInfo> result = vc.GetStats(app);
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            return true;
        }
    }
}
#endif