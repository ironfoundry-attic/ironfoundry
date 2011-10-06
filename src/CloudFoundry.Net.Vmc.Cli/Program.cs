namespace CloudFoundry.Net.Vmc.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NDesk.Options;

    static partial class Program
    {
        static int verbosity           = 0;
        static bool show_help          = false;
        static bool result_as_json     = false;
        static bool result_as_rawjson  = false;
        static string command_url      = null;
        static string command_email    = null;
        static string command_password = null;
        static bool prompt_ok          = true;
        static ushort instances        = 1;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var commands = new Dictionary<string, Func<IList<string>, bool>>
            {
                { "help",           (arg) => Usage() },
                { "info",           (arg) => Info(arg) },
                { "target",         (arg) => Target(arg) },
                { "login",          (arg) => Login(arg) },
                { "push",           (arg) => Push(arg) },
                { "services",       (arg) => Services(arg) },
                { "bind-service",   (arg) => BindService(arg) },
                { "create-service", (arg) => CreateService(arg) },
                { "delete-service", (arg) => DeleteService(arg) },
                { "unbind-service", (arg) => UnbindService(arg) },
                { "delete",         (arg) => Delete(arg) },
            };

            var p = new OptionSet
            {
                { "v|verbose", "increase verbosity", v =>
                    { if (false == String.IsNullOrEmpty(v)) ++verbosity; } },

                { "h|help", "show help", v => { show_help = null != v; } },

                { "json", "show result as json", v => { result_as_json = null != v; } },

                { "rawjson", "show result as raw json", v => { result_as_rawjson = null != v; } },

                { "url=", "set command url", v => { command_url = v; } },
                
                { "email=", "set command email", v => { command_email = v; } },
                
                { "passwd=", "set command password", v => { command_password = v; } },

                { "noprompts", "set prompting", v => { prompt_ok = v.IsNullOrWhiteSpace(); } },

                { "instances=", "set instances", v => { instances = Convert.ToUInt16(v); } },
            };

            IList<string> unparsed = null;
            try
            {
                unparsed = p.Parse(args);
            }
            catch (OptionException)
            {
                Usage();
            }

            bool success = true;
            if (show_help)
            {
                Usage(p);
            }
            else
            {
                if (false == unparsed.IsNullOrEmpty())
                {
                    string verb = unparsed[0];
                    Func<IList<string>, bool> action;
                    if (commands.TryGetValue(verb, out action))
                    {
                        try
                        {
                            success = action(unparsed.Skip(1).ToList());
                        }
                        catch (Exception e)
                        {
                            success = false;
                            Console.Error.WriteLine(e.Message);
                        }
                    }
                    else
                    {
                        Usage(p);
                    }
                }
                else
                {
                    Usage(p);
                }
            }

            Environment.Exit(success ? 0 : 1);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Error.WriteLine("Unhandled exception!");
            var ex = e.ExceptionObject as Exception;
            if (null != ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
            Environment.Exit(1);
        }
    }
}