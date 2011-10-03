namespace CloudFoundry.Net.Vmc.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NDesk.Options;
    using Newtonsoft.Json;
    using Properties;
    using Types;

    static class Program
    {
        static int verbosity = 0;
        static bool show_help = false;
        static bool result_as_json = false;
        static bool result_as_rawjson = false;
        static string command_url = null;

        static void Main(string[] args)
        {
            var commands = new Dictionary<string, Action<IList<string>>>
            {
                { "help", (arg)   => usage() },
                { "info", (arg)   => info(arg) },
                { "target", (arg) => target(arg) },
                { "login", (arg)  => login(arg) },
            };

            var p = new OptionSet
            {
                { "v|verbose", "increase verbosity", v =>
                    { if (false == String.IsNullOrEmpty(v)) ++verbosity; } },

                { "h|help", "show help", v => { show_help = null != v; } },

                { "json", "show result as json", v => { result_as_json = null != v; } },

                { "rawjson", "show result as raw json", v => { result_as_rawjson = null != v; } },

                { "url", "set command url", v => { command_url = v; } },
            };

            IList<string> unparsed = null;
            try
            {
                unparsed = p.Parse(args);
            }
            catch (OptionException)
            {
                Console.WriteLine(Usage.BASIC_USAGE);
            }

            if (show_help)
            {
                showHelp(p);
            }

            if (false == unparsed.IsNullOrEmpty())
            {
                string verb = unparsed[0];
                Action<IList<string>> action;
                if (commands.TryGetValue(verb, out action))
                {
                    action(unparsed.Skip(1).ToList());
                }
                else
                {
                    showHelp(p);
                }
            }
            else
            {
                showHelp(p);
            }
        }

        static void info(IList<string> unparsed)
        {
            var vc = new VcapClient();
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
                    Console.WriteLine(String.Format(Resources.VcapClient_InfoDisplay_1_Fmt,
                        info.Description, info.Support, vc.CurrentUri, info.Version, "TODO CLIENT VERSION"));

                    if (false == info.User.IsNullOrEmpty())
                    {
                        Console.WriteLine(String.Format(Resources.VcapClient_InfoDisplay_2_Fmt, info.User));
                    }

                    if (null != info.Usage && null != info.Limits)
                    {
                        string tmem = pretty_size(info.Limits.Memory * 1024 * 1024);
                        string mem = pretty_size(info.Usage.Memory * 1024 * 1024);

                        Console.WriteLine(String.Format(Resources.VcapClient_InfoDisplay_3_Fmt,
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
        }

        static void target(IList<string> unparsed)
        {
            string url = null;
            if (command_url.IsNullOrWhiteSpace())
            {
                if (false == (unparsed.IsNullOrEmpty() || unparsed[0].IsNullOrWhiteSpace()))
                {
                    url = unparsed[0];
                }
            }
            else
            {
                url = command_url;
            }

            var vc = new VcapClient();
            VcapClientResult rslt = vc.Target(url);
            if (rslt.Success)
            {
                Console.WriteLine(String.Format(Resources.VcapClient_TargetDisplay_Fmt, vc.CurrentUri));
            }
            else
            {
                Console.WriteLine(String.Format(Resources.VcapClient_TargetNoSuccessDisplay_Fmt, vc.CurrentUri));
            }
        }

        static void login(IList<string> unparsed)
        {
        }

        static void usage()
        {
            Console.WriteLine(Usage.COMMAND_USAGE);
        }

        static void showHelp(OptionSet p)
        {
            Console.WriteLine("Usage: vmc [OPTIONS]+ message");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static void debug(string format, params object[] args)
        {
            if (verbosity > 0)
            {
                Console.Write ("# ");
                Console.WriteLine (format, args);
            }
        }

        static string pretty_size(uint argSize, ushort argPrec = 1)
        {
            if (argSize == 0)
            {
                return "NA";
            }

            if (argSize < 1024)
            {
                return String.Format("{0}B", argSize);
            }

            if (argSize < (1024*1024))
            {
                return String.Format("{0:F" + argPrec + "}K", argSize / 1024.0);
            }

            if (argSize < (1024*1024*1024))
            {
                return String.Format("{0:F" + argPrec + "}M", argSize / 1024.0);
            }

            return String.Format("{0:F" + argPrec + "}G", argSize / (1024.0 * 1024.0 * 1024.0));
        }

            /*
            if (args.Length == 0)
            {
                Console.WriteLine("Currently only login, info and push commands are supported");
            }
             else if (args[0] == "Target")
            {
                
                writeTargetFile(args[1]);
                Console.WriteLine("Target set to: {0}", args[1]);
            }

            else if (args[0] == "Info")
            {
            }
            else if (args[0] == "Login")
            {
                url = readTargetFile();
                if (url == String.Empty)
                {
                    Console.WriteLine("Please set a target");
                }
                else
                {
                    Console.Write("Email: ");
                    string email = Console.ReadLine();
                    Console.Write("Password: ");
                    string password = Console.ReadLine(); //should figure out how to turn this to *
                    VmcManager cfm = new VmcManager();
                    cfm.URL = url;
                    string returnvalue = cfm.LogIn(email, password);
                    if (returnvalue.Contains("token"))
                    {
                        writeTokenFile(returnvalue.Replace("token",url));
                    }
                    else
                    {
                        Console.WriteLine("Login Failed");
                    }
                }
            }
            else if (args[0] == "Push")
            {
                url = readTargetFile();
                accesstoken = readTokenFile();
                if (url == String.Empty)
                {
                    Console.WriteLine("Please set a target");
                }
                else if (accesstoken.Length > 0)
                {
                    Console.Write("App Name: ");
                    string appname = "johndoe" ; //Console.ReadLine();
                    Console.Write("Directory Location (ex. c:\\appdir ): ");
                    string dirlocation = "c:\\testapp"; //Console.ReadLine();
                    Console.Write("Deployed URL (ex. xyz.cloudfoundry.com) ");
                    string deployedURL = "johndoe.cloudfoundry.com"; //Console.ReadLine();
                    Console.Write("Type of Application: (ex. sinatra, java) ");
                    string apptype = "sinatra"; //Console.ReadLine();
                    Console.Write("Memory Reservation: (ex. 128) ");
                    string memalloc = "128"; //Console.ReadLine();

                    JObject obj = JObject.Parse(accesstoken);
                    VmcManager cfm = new VmcManager();
                    cfm.AccessToken = (string)obj.Value<string>(url);
                    cfm.URL = url;
                    Console.Write("Return data: ");
                    Console.WriteLine(cfm.Push(appname, deployedURL, dirlocation, apptype, memalloc));
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Please login first.");
                }
            }
             */
    }
}