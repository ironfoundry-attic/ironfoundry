namespace IronFoundry.Warden.Utilities.Impersonation
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    public static class Impersonator
    {
        public static WindowsImpersonationContext GetContext(NetworkCredential credential, bool shouldImpersonate = false)
        {
            if (!shouldImpersonate)
            {
                return WindowsIdentity.GetCurrent().Impersonate();
            }

            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            try
            {
                if (NativeMethods.RevertToSelf())
                {
                    if (NativeMethods.LogonUser(
                        credential.UserName,
                        credential.UserName.Contains("@") ? null : credential.Domain, // if UPN format, don't use domain name
                        credential.Password,
                        (int)ImpersonationLogonType.Interactive,
                        Constants.LOGON32_PROVIDER_DEFAULT,
                        ref token) != 0)
                    {
                        if (NativeMethods.DuplicateToken(token, Constants.SECURITY_IMPERSONATION, ref tokenDuplicate) != 0)
                        {
                            return WindowsIdentity.Impersonate(tokenDuplicate);
                        }
                        else
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                    }
                    else
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(token);
                }
                if (tokenDuplicate != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(tokenDuplicate);
                }
            }
        }

        private class NativeMethods
        {
            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern int LogonUser(
                string lpszUserName,
                string lpszDomain,
                string lpszPassword,
                int dwLogonType,
                int dwLogonProvider,
                ref IntPtr phToken);

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool RevertToSelf();

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern bool CloseHandle(IntPtr handle);
        }

        /// <summary>
        /// The LOGON32_LOGON type.
        /// </summary>
        private enum ImpersonationLogonType
        {
            /// <summary>
            /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on  
            /// by a terminal server, remote shell, or similar process.
            /// This logon type has the additional expense of caching logon information for disconnected operations; 
            /// therefore, it is inappropriate for some client/server applications,
            /// such as a mail server. (LOGON32_LOGON_INTERACTIVE)
            /// </summary>
            Interactive = 2,

            /// <summary>
            /// This logon type is intended for high performance servers to authenticate plaintext passwords.
            /// The LogonUser function does not cache credentials for this logon type.
            /// (LOGON32_LOGON_NETWORK)
            /// </summary>
            Network = 3,

            /// <summary>
            /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without 
            /// their direct intervention. This type is also for higher performance servers that process many plaintext
            /// authentication attempts at a time, such as mail or Web servers. 
            /// The LogonUser function does not cache credentials for this logon type.
            /// (LOGON32_LOGON_BATCH)
            /// </summary>
            Batch = 4,

            /// <summary>
            /// Indicates a service-type logon. The account provided must have the service privilege enabled. (LOGON32_LOGON_SERVICE)
            /// </summary>
            Service = 5,

            /// <summary>
            /// This logon type is for GINA DLLs that log on users who will be interactively using the computer. 
            /// This logon type can generate a unique audit record that shows when the workstation was unlocked.
            /// (LOGON32_LOGON_UNLOCK)
            /// </summary>
            Unlock = 7,

            /// <summary>
            /// This logon type preserves the name and password in the authentication package, which allows the server to make 
            /// connections to other network servers while impersonating the client. A server can accept plaintext credentials 
            /// from a client, call LogonUser, verify that the user can access the system across the network, and still 
            /// communicate with other servers.
            /// NOTE: Windows NT:  This value is not supported.
            /// (LOGON32_LOGON_NETWORK_CLEARTEXT)
            /// </summary>
            ClearText = 8,

            /// <summary>
            /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
            /// The new logon session has the same local identifier but uses different credentials for other network connections. 
            /// NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
            /// NOTE: Windows NT:  This value is not supported.
            /// (LOGON32_LOGON_NEW_CREDENTIALS)
            /// </summary>
            NewCredentials = 9,
        }

        private static class Constants
        {
            public const int SECURITY_IMPERSONATION = 2;

            /// <summary>
            /// Use the standard logon provider for the system. 
            /// The default security provider is negotiate, unless you pass NULL for the domain name and the user name 
            /// is not in UPN format. In this case, the default provider is NTLM. 
            /// NOTE: Windows 2000/NT:   The default security provider is NTLM.
            /// </summary>
            public const int LOGON32_PROVIDER_DEFAULT = 0;
            public const int LOGON32_LOGON_INTERACTIVE = 2;
            public const int LOGON32_PROVIDER_WINNT50 = 3;
            public const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        }
    }
}
