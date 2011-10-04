namespace CloudFoundry.Net.Vmc.Cli
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NDesk.Options;
    using Newtonsoft.Json;
    using Properties;
    using Types;

    static class Program
    {
        static int verbosity           = 0;
        static bool show_help          = false;
        static bool result_as_json     = false;
        static bool result_as_rawjson  = false;
        static string command_url      = null;
        static string command_email    = null;
        static string command_password = null;
        static bool prompt_ok          = true;

        static void Main(string[] args)
        {
            var commands = new Dictionary<string, Func<IList<string>, bool>>
            {
                { "help",   (arg) => usage() },
                { "info",   (arg) => info(arg) },
                { "target", (arg) => target(arg) },
                { "login",  (arg) => login(arg) },
                { "push",   (arg) => push(arg) },
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

            bool success = false;
            if (false == unparsed.IsNullOrEmpty())
            {
                string verb = unparsed[0];
                Func<IList<string>, bool> action;
                if (commands.TryGetValue(verb, out action))
                {
                    success = action(unparsed.Skip(1).ToList());
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

            Environment.Exit(success ? 0 : 1);
        }

        static bool info(IList<string> unparsed)
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
                    Console.WriteLine(String.Format(Resources.Vmc_InfoDisplay_1_Fmt,
                        info.Description, info.Support, vc.CurrentUri, info.Version, "TODO CLIENT VERSION"));

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

        static bool target(IList<string> unparsed)
        {
            string url = command_url;
            if (false == unparsed.IsNullOrEmpty())
            {
                url = unparsed[0];
            }

            var vc = new VcapClient();
            VcapClientResult rslt = vc.Target(url);
            if (rslt.Success)
            {
                Console.WriteLine(String.Format(Resources.Vmc_TargetDisplay_Fmt, vc.CurrentUri));
            }
            else
            {
                Console.WriteLine(String.Format(Resources.Vmc_TargetNoSuccessDisplay_Fmt, vc.CurrentUri));
            }

            return rslt.Success;
        }

        static bool login(IList<string> unparsed)
        {
            bool failed = true;
            ushort tries = 0;

            while (failed && tries < 3)
            {
                string email = command_email;
                if (false == unparsed.IsNullOrEmpty())
                {
                    email = unparsed[0];
                }
                if (prompt_ok && email.IsNullOrWhiteSpace())
                {
                    Console.Write(Resources.Vmc_EmailPrompt_Text);
                    email = Console.ReadLine();
                }

                string password = command_password;
                if (prompt_ok && password.IsNullOrWhiteSpace())
                {
                    Console.Write(Resources.Vmc_PasswordPrompt_Text);

                    var passwordList = new LinkedList<char>();
                    bool reading_pwd = true;
                    while (reading_pwd)
                    {
                        ConsoleKeyInfo info = Console.ReadKey(true);
                        switch (info.Key)
                        {
                            case ConsoleKey.Enter :
                                reading_pwd = false;
                                break;
                            case ConsoleKey.Delete :
                            case ConsoleKey.Backspace :
                                if (false == passwordList.IsNullOrEmpty())
                                {
                                    Console.Write("\b \b");
                                    passwordList.RemoveLast();
                                }
                                break;
                            default :
                                passwordList.AddLast(info.KeyChar);
                                Console.Write('*');
                                break;
                        }
                    }

                    password = String.Join("", passwordList);
                }

                Console.WriteLine();

                if (email.IsNullOrWhiteSpace())
                {
                    Console.Error.WriteLine(Resources.Vmc_NeedEmailPrompt_Text);
                    return false;
                }

                if (password.IsNullOrWhiteSpace())
                {
                    Console.Error.WriteLine(Resources.Vmc_NeedPasswordPrompt_Text);
                    return false;
                }

                var vc = new VcapClient();
                try
                {
                    VcapClientResult rslt = vc.Login(email, password);
                    if (rslt.Success)
                    {
                        Console.WriteLine(String.Format(Resources.Vmc_LoginSuccess_Fmt, vc.CurrentUri));
                        failed = false;
                    }
                    else
                    {
                        Console.Error.WriteLine(String.Format(Resources.Vmc_LoginFail_Fmt, vc.CurrentUri));
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(String.Format(Resources.Vmc_LoginError_Fmt, vc.CurrentUri, e.Message));
                }

                // TODO retry if (tries += 1) < 3 && prompt_ok && !@options[:password]
                ++tries;
            }

            return false == failed;
        }

        static bool usage()
        {
            Console.WriteLine(Usage.COMMAND_USAGE);
            return true;
        }

        static bool showHelp(OptionSet p)
        {
            Console.WriteLine("Usage: vmc [OPTIONS]+ message");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            return true;
        }

        static bool push(IList<string> unparsed)
        {
            // TODO match ruby argument parsing
            if (unparsed.Count != 3)
            {
                Console.Error.WriteLine("Usage: vmc push appname path url"); // TODO usage statement standardization
                return false;
            }

            string appname = unparsed[0];
            string path    = unparsed[1];
            string url     = unparsed[2];

            DirectoryInfo di = null;
            if (Directory.Exists(path))
            {
                di = new DirectoryInfo(path);
            }
            else
            {
                Console.Error.WriteLine(String.Format("Directory '{0}' does not exist."));
                return false;
            }

            var vc = new VcapClient();
            VcapClientResult rv = vc.Push(appname, url, di, 64);
            if (false == rv.Success)
            {
                Console.Error.WriteLine(rv.Message);
            }
            return rv.Success;
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
    }
}