namespace IronFoundry.Vmc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IronFoundry.Types;
    using IronFoundry.Vcap;

    static partial class Program
    {
        static bool Passwd(IList<string> unparsed)
        {
            if (unparsed.Count != 0)
            {
                Console.Error.WriteLine("Too many arguments for [change_password]: {0}", String.Join(", ", unparsed.Select(s => String.Format("'{0}'", s))));
                Console.Error.WriteLine("Usage: vmc passwd");
                return false;
            }

            IVcapClient vc = new VcapClient();
            VcapClientResult rslt = vc.Info();
            Info info = rslt.GetResponseMessage<Info>();

            Console.WriteLine("Changing password for '{0}'", info.User);

            Console.Write("New Password: ");
            string newPassword = readPassword();
            Console.WriteLine();
            Console.Write("Verify Password: ");
            string verifyPassword = readPassword();

            if (newPassword == verifyPassword)
            {
                vc.ChangePassword(newPassword);
                return true;
            }
            else
            {
                Console.Error.WriteLine("Passwords did not match!");
                return false;
            }
        }
    }
}