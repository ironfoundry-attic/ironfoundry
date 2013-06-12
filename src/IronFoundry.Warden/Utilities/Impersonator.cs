using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Warden.Utilities
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    public static class Impersonator
    {
        private const int GENERIC_ALL_ACCESS = 0x10000000;

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
                        var sa = new SECURITY_ATTRIBUTES { bInheritHandle = true };
                        sa.Length = Marshal.SizeOf(sa);

                        if (NativeMethods.DuplicateTokenEx(
                            token,
                            GENERIC_ALL_ACCESS,
                            ref sa,
                            SECURITY_IMPERSONATION_LEVEL.SecurityIdentification,
                            TOKEN_TYPE.TokenPrimary,
                            out tokenDuplicate))
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

        private static class NativeMethods
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
            public static extern bool DuplicateTokenEx(
                IntPtr hExistingToken,
                uint dwDesiredAccess,
                ref SECURITY_ATTRIBUTES lpTokenAttributes,
                SECURITY_IMPERSONATION_LEVEL impersonationLevel,
                TOKEN_TYPE tokenType,
                out IntPtr hNewToken);

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
            Interactive = 2,
            Network = 3,
            Batch = 4,
            Service = 5,
            Unlock = 7,
            ClearText = 8,
            NewCredentials = 9,
        }

        private enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous      = 0,
            SecurityIdentification = 1,
            SecurityImpersonation  = 2,
            SecurityDelegation     = 3
        }

        private enum TOKEN_TYPE
        {
            TokenPrimary       = 1, 
            TokenImpersonation = 2
        } 

        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public Int32 Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        private static class Constants
        {
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
