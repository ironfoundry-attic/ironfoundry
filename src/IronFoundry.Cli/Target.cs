namespace IronFoundry.Cli
{
    using System;
    using System.Collections.Generic;
    using IronFoundry.Properties;
    using IronFoundry.Vcap;

    static partial class Program
    {
        static bool Target(IList<string> unparsed)
        {
            string url = command_url;
            if (false == unparsed.IsNullOrEmpty())
            {
                url = unparsed[0];
            }

            IVcapClient vc = new VcapClient();
            VcapClientResult rslt = vc.Target(url);
            if (rslt.Success)
            {
                Console.WriteLine(String.Format(Resources.Vmc_TargetDisplay_Fmt, rslt.Message));
            }
            else
            {
                Console.WriteLine(String.Format(Resources.Vmc_TargetNoSuccessDisplay_Fmt, rslt.Message));
            }

            return rslt.Success;
        }
    }
}