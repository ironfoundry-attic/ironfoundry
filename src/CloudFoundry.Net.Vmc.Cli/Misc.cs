namespace CloudFoundry.Net.Vmc.Cli
{
    using System;

    static partial class Program
    {
        static void debug(string format, params object[] args)
        {
            if (verbosity > 0)
            {
                Console.Write("# ");
                Console.WriteLine(format, args);
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
    }
}