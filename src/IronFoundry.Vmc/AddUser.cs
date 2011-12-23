namespace IronFoundry.Vmc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IronFoundry.Vcap;

    static partial class Program
    {
        static bool AddUser(IList<string> unparsed)
        {
            if (unparsed.Count != 0)
            {
                Console.Error.WriteLine("Too many arguments for [add_user]: {0}", String.Join(", ", unparsed.Select(s => String.Format("'{0}'", s))));
                Console.Error.WriteLine("Usage: vmc add-user");
                return false;
            }

            Console.Write("Email: ");
            string email = Console.ReadLine();

            Console.Write("Password: ");
            string password = readPassword();
            Console.WriteLine();
            Console.Write("Verify Password: ");
            string verifyPassword = readPassword();

            if (password == verifyPassword)
            {
                IVcapClient vc = new VcapClient();
                Console.WriteLine();
                Console.Write("Creating New User: ");
                vc.AddUser(email, password);
                Console.WriteLine("OK");
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