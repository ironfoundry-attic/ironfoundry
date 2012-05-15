using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronFoundry.Vcap;
using IronFoundry.Types;

namespace TempTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new VcapClient("http://api.wfsroy.qa1.wfabricqa.com");

            try
            {
                client.DeleteUser("eleetest@ironfoundry.org");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
