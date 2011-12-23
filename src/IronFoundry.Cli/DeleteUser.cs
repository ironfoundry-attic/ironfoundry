namespace IronFoundry.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IronFoundry.Vcap;

    static partial class Program
    {
        static bool DeleteUser(IList<string> unparsed)
        {
            if (unparsed.Count != 1)
            {
                Console.Error.WriteLine("Too many arguments for [delete_user]: {0}", String.Join(", ", unparsed.Select(s => String.Format("'{0}'", s))));
                Console.Error.WriteLine("Usage: vmc delete-user <user>");
                return false;
            }

            string email = unparsed[0];
            Console.WriteLine("Deleting user '{0}' will also delete all applications and services for this user.", email);
            Console.Write("Do you want to do this? [yN] ");
            string input = Console.ReadLine();
            if (input == "y")
            {
                Console.Write("Deleting User '{0}' ... ", email);
                IVcapClient vc = new VcapClient();
                VcapClientResult result = vc.DeleteUser(email);
                Console.WriteLine("OK");
            }
            return true;
        }
    }
}