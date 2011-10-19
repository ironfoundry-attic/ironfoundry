namespace CloudFoundry.Net.Vmc.Cli
{
    using System;
    using System.Collections.Generic;
    using Types;
    using CloudFoundry.Net.Extensions;

    static partial class Program
    {
        static bool Apps(IList<string> unparsed)
        {
            // TODO match ruby argument parsing
            if (unparsed.Count != 0)
            {
                Console.Error.WriteLine("Too many arguments for [apps]");
                Console.Error.WriteLine("Usage: vmc apps"); // TODO usage statement standardization
                return false;
            }
            IVcapClient vc = new VcapClient();
            IEnumerable<Application> apps = vc.GetApplications();
            if (false == apps.IsNullOrEmpty())
            {
                foreach (Application a in apps)
                {
                    Console.WriteLine("App name: {0} Instances: {1} State: {2} Services: {3}",
                        a.Name, a.RunningInstances, a.State, String.Join(", ", a.Services));
                }
            }
            return true;
        }
    }
}