namespace IronFoundry.Cli
{
    using System;
    using System.Collections.Generic;

    static partial class Program
    {
        static void Debug(string format, params object[] args)
        {
            if (verbosity > 0)
            {
                Console.Write("# ");
                Console.WriteLine(format, args);
            }
        }

        static string PrettySize(uint argSize, ushort argPrec = 1)
        {
            if (argSize == 0)
            {
                return "NA";
            }

            if (argSize < 1024)
            {
                return String.Format("{0}B", argSize);
            }

            if (argSize < (1024 * 1024))
            {
                return String.Format("{0:F" + argPrec + "}K", argSize / 1024.0);
            }

            if (argSize < (1024 * 1024 * 1024))
            {
                return String.Format("{0:F" + argPrec + "}M", argSize / 1024.0);
            }

            return String.Format("{0:F" + argPrec + "}G", argSize / (1024.0 * 1024.0 * 1024.0));
        }

        private static string readPassword()
        {
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
            return String.Join("", passwordList);
        }
    }
}