namespace CloudFoundry.Net.Vmc.Cli
{
    // http://stackoverflow.com/questions/125319/should-usings-be-inside-or-outside-the-namespace
    using System;
    using System.Collections.Generic;
    using NDesk.Options;

    static class Program
    {
        static int verbosity = 0;
        static bool show_help = false;
        static string url = String.Empty;
        static string accesstoken = String.Empty;

        static void Main(string[] args)
        {
            var commands = new Dictionary<string, Action<IList<string>>>
            {
                { "help", (arg) => usage() },
                { "info", (arg) => info(arg) },
                    /*
                "info",
                "target",
                "login"
                     */
            };

            var p = new OptionSet
            {
                { "v|verbose", "increase verbosity", v =>
                    { if (false == String.IsNullOrEmpty(v)) ++verbosity; } },

                { "h|help", "show help", v => { show_help = null != v; } },
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

            if (false == unparsed.IsNullOrEmpty() && 1 == unparsed.Count)
            {
                string verb = unparsed[0];
                Action<IList<string>> action;
                if (commands.TryGetValue(verb, out action))
                {
                    action(unparsed);
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
                Console.WriteLine(rslt.Message);
            }
            else
            {
                // TODO standardize errors
                Console.Error.WriteLine(String.Format("Error: {0}", rslt.Message));
            }
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