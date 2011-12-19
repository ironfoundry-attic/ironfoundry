namespace IronFoundry.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using IronFoundry.Properties;
    using IronFoundry.Types;
    using IronFoundry.Vcap;
    using Newtonsoft.Json;

    static partial class Program
    {
        static bool Info(IList<string> unparsed)
        {
            // TODO match ruby argument parsing
            if (unparsed.Count != 0)
            {
                Console.Error.WriteLine("Usage: vmc info"); // TODO usage statement standardization
                return false;
            }

            IVcapClient vc = new VcapClient();
            VcapClientResult rslt = vc.Info();
            if (rslt.Success)
            {
                var info = rslt.GetResponseMessage<Info>();
                if (result_as_json || result_as_rawjson)
                {
                    if (result_as_rawjson)
                    {
                        Console.WriteLine(info.RawJson);
                    }
                    if (result_as_json)
                    {
                        Console.WriteLine(JsonConvert.SerializeObject(info, Formatting.Indented));
                    }
                }
                else
                {
                    Version ver = Assembly.GetExecutingAssembly().GetName().Version;

                    Console.WriteLine(String.Format(Resources.Vmc_InfoDisplay_1_Fmt,
                        info.Description, info.Support, vc.CurrentUri, info.Version, ver));

                    if (false == info.User.IsNullOrEmpty())
                    {
                        Console.WriteLine(String.Format(Resources.Vmc_InfoDisplay_2_Fmt, info.User));
                    }

                    if (null != info.Usage && null != info.Limits)
                    {
                        string tmem = pretty_size(info.Limits.Memory * 1024 * 1024);
                        string mem = pretty_size(info.Usage.Memory * 1024 * 1024);

                        Console.WriteLine(String.Format(Resources.Vmc_InfoDisplay_3_Fmt,
                            mem, tmem,
                            info.Usage.Services, info.Limits.Services,
                            info.Usage.Apps, info.Limits.Apps));
                    }
                }
            }
            else
            {
                // TODO standardize errors
                Console.Error.WriteLine(String.Format("Error: {0}", rslt.Message));
            }

            return rslt.Success;
        }
    }
}