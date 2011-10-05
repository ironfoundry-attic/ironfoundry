namespace CloudFoundry.Net.Vmc.Cli
{
    using System;
    using System.Collections.Generic;
    using CloudFoundry.Net.Types;

    static partial class Program
    {
        const string systemHeader = @"============== System Services ==============";
        const string provisionedHeader = @"=========== Provisioned Services ============";

        static bool services(IList<string> unparsed)
        {
            var vc = new VcapClient();
            IEnumerable<SystemService> systemServices = vc.GetSystemServices();
            if (false == systemServices.IsNullOrEmpty())
            {
                Console.WriteLine(systemHeader);
                foreach (SystemService s in systemServices)
                {
                    Console.WriteLine("{0}     {1}     {2}", s.Vendor, s.Version, s.Description);
                }
            }

            IEnumerable<ProvisionedService> provisionedServices = vc.GetProvisionedServices();
            if (false == provisionedServices.IsNullOrEmpty())
            {
                Console.WriteLine();
                Console.WriteLine(provisionedHeader);
                foreach (ProvisionedService s in provisionedServices)
                {
                    Console.WriteLine("{0}     {1}", s.Name, s.Vendor);
                }
            }

            return true;
        }
    }
}