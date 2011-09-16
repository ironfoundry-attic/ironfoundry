using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.Vmc
{
    internal class VmcInfo
    {
        public void GetCrashes () {
            //GET /apps/sroytest1/crashes
        }

        public void GetCrashLogs() {
            //GET /apps/sroytest1/instances/0/files/logs/stderr.log
            //GET /apps/sroytest1/instances/0/files/logs/stdout.log
            //Get /apps/sroytest1/instances/0/files/logs/startup.log
        }

        public void GetLogs() {
            //GET /apps/sroytest1/instances/0/files/logs/stderr.log
            //GET /apps/sroytest1/instances/0/files/logs/stdout.log
            //Get /apps/sroytest1/instances/0/files/logs/startup.log
        }

        public void GetFiles() {

        }

        public void GetStats() {

        }

        public void GetInstances() {

        }
    }
}
