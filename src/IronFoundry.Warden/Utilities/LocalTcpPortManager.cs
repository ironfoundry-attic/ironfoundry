namespace IronFoundry.Warden.Utilities
{
    using System;
    using NLog;

    public class LocalTcpPortManager
    {
        private static readonly string workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System);

        private readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// netsh http add urlacl http://*:8888/ user=warden_094850238
        /// </summary>
        public ushort ReserveLocalPort(ushort port, string userName)
        {
            ushort portToReserve = port;
            if (portToReserve == default(ushort))
            {
                portToReserve = IPUtilities.RandomFreePort();
            }

            string arguments = String.Format("http add urlacl http://*:{0}/ user={1}", portToReserve, userName);
            if (!RunNetsh(arguments))
            {
                throw new WardenException("Error reserving port '{0}' for user '{1}'", portToReserve, userName);
            }

            return portToReserve;
        }

        /// <summary>
        /// netsh http delete urlacl http://*:8888/
        /// </summary>
        public void ReleaseLocalPort(ushort port)
        {
            string arguments = String.Format("http delete urlacl http://*:{0}/", port);
            if (!RunNetsh(arguments))
            {
                throw new WardenException("Error removing reservation for port '{0}'", port);
            }
        }

        private bool RunNetsh(string arguments)
        {
            bool success = false;

            using (var process = new BackgroundProcess(workingDirectory, "netsh.exe", arguments))
            {
                process.StartAndWait(asyncOutput: false);
                /*
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                log.Trace("stdout: '{0}' stderr: '{1}'", stdout, stderr);
                */
                success = process.ExitCode == 0;
            }

            return success;
        }
    }
}
