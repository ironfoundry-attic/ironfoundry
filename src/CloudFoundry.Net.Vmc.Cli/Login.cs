namespace CloudFoundry.Net.Vmc.Cli
{
    using System;
    using System.Collections.Generic;
    using Properties;

    static partial class Program
    {
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
                            case ConsoleKey.Enter:
                                reading_pwd = false;
                                break;
                            case ConsoleKey.Delete:
                            case ConsoleKey.Backspace:
                                if (false == passwordList.IsNullOrEmpty())
                                {
                                    Console.Write("\b \b");
                                    passwordList.RemoveLast();
                                }
                                break;
                            default:
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
    }
}