namespace IronFoundry.Warden.PInvoke
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Runtime.InteropServices;

    internal static class Utils
    {
        public static HandleRef GetCurrentProcessRef(object wrapper)
        {
            IntPtr currentProcess = NativeMethods.GetCurrentProcess();
            return new HandleRef(wrapper, currentProcess);
        }

        public static SafeUserTokenHandle LogonAndGetUserPrimaryToken(NetworkCredential credential)
        {
            IntPtr token = IntPtr.Zero;
            IntPtr primaryToken = IntPtr.Zero;

            try
            {
                if (NativeMethods.RevertToSelf())
                {
                    if (NativeMethods.LogonUser(credential.UserName, ".", credential.Password,
                        NativeMethods.LogonType.LOGON32_LOGON_INTERACTIVE,
                        NativeMethods.LogonProvider.LOGON32_PROVIDER_DEFAULT,
                        out token))
                    {
                        var sa = new NativeMethods.SecurityAttributes();

                        if (NativeMethods.DuplicateTokenEx(
                            token,
                            NativeMethods.Constants.GENERIC_ALL_ACCESS,
                            sa,
                            NativeMethods.SecurityImpersonationLevel.SecurityImpersonation,
                            NativeMethods.TokenType.TokenPrimary,
                            out primaryToken))
                        {
                            return new SafeUserTokenHandle(primaryToken);
                        }
                        else
                        {
                            throw new Win32Exception();
                        }
                    }
                    else
                    {
                        throw new Win32Exception();
                    }
                }
                else
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(token);
                }
            }
        }
    }
}
