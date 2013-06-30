using System;

namespace IronFoundry.Warden.Service.InstallerCA.Win32
{
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    class LogonUser
    {
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private readonly ILogger logger;

        public LogonUser(ILogger logger)
        {
            this.logger = logger;
        }

        public Boolean ValidateCredentials(string userName, string password)
        {
            if (userName == "LocalSystem") return true;

            return Logon(userName, password, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT);
        }

        private Boolean Logon(string userName, string password, int logonType, int provider)
        {
            SafeFileHandle userTokenHandle = null;

            try
            {
                var parsedData = ParseUserName(userName);
                if (Win32.LogonUser(parsedData.Item1, parsedData.Item2, password, logonType, provider, out userTokenHandle))
                {
                    return true;
                }

                throw new Win32Exception();
            }
            catch (Exception ex)
            {
                logger.Log("Error: {0}", ex.ToString());
                return false;
            }
            finally
            {
                if (userTokenHandle != null)
                {
                    userTokenHandle.Close();
                }
            }
        }

        public static Tuple<string, string> ParseUserName(string input)
        {
            string userName = null;
            string domain = null;

            int delimiterIdx = input.IndexOf('\\');
            if (delimiterIdx > -1)
            {
                domain = input.Substring(0, delimiterIdx);
                int skipIdx = delimiterIdx + 1;
                userName = input.Substring(skipIdx, input.Length - skipIdx);
            }
            else
            {
                userName = input;
                domain = null;
            }

            return new Tuple<string, string>(userName, domain);
        }

        private static class Win32
        {
            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool LogonUser(
                string lpszUsername,
                string lpszDomain,
                string lpszPassword,
                int dwLogonType,
                int dwLogonProvider,
                out SafeFileHandle phToken
                );
        }
    }
}
