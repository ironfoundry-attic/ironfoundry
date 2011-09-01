using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Dea.Providers;
using System.Threading.Tasks;

namespace CloudFoundry.Net.Dea.Service
{
    class Program
    {
        public static void Main(string[] args)
        {
            Agent agent = new Agent();
            agent.Run();                        
        }
    }
}
