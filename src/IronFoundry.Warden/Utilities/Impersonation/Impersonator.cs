namespace IronFoundry.Warden.Utilities.Impersonation
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Threading.Tasks;

    public static class Impersonator
    {
        #region Asynchronous

        public static async Task InvokeImpersonatedAsync(NetworkCredential credential, Action action)
        {
            await InvokeImpersonatedAsync(credential, ImpersonationLogonType.NewCredentials, action);
        }

        public static async Task<T> InvokeImpersonatedAsync<T>(NetworkCredential credential, Func<Task<T>> function)
        {
            return await InvokeImpersonatedAsync(credential, ImpersonationLogonType.NewCredentials, function);
        }

        private static async Task InvokeImpersonatedAsync(NetworkCredential credential, ImpersonationLogonType logonType, Action action)
        {
            WindowsImpersonationContext context = await GetContextAsync(credential, logonType);

            try
            {
                action();
            }
            finally
            {
                if (context != null)
                {
                    context.Undo();
                    context.Dispose();
                }
            }
        }

        private static async Task<T> InvokeImpersonatedAsync<T>(NetworkCredential credential, ImpersonationLogonType logonType, Func<Task<T>> function)
        {
            WindowsImpersonationContext context = await GetContextAsync(credential, logonType);

            try
            {
                return await function();
            }
            finally
            {
                if (context != null)
                {
                    context.Undo();
                    context.Dispose();
                }
            }
        }

        private static async Task<WindowsImpersonationContext> GetContextAsync(NetworkCredential credential, ImpersonationLogonType logonType)
        {
            return await Task.Run(() => GetContext(credential, logonType));
        }
        #endregion

        #region Synchronous

        public static T InvokeImpersonated<T>(NetworkCredential credential, Func<T> function)
        {
            return InvokeImpersonated(credential, ImpersonationLogonType.NewCredentials, function);
        }

        public static void InvokeImpersonated(NetworkCredential credential, Action action)
        {
            InvokeImpersonated(credential, ImpersonationLogonType.NewCredentials, action);
        }

        private static void InvokeImpersonated(NetworkCredential credential, ImpersonationLogonType logonType, Action action)
        {
            var context = GetContext(credential, logonType);

            try
            {
                action();
            }
            finally
            {
                if (context != null)
                {
                    context.Undo();
                    context.Dispose();
                }
            }
        }

        private static T InvokeImpersonated<T>(NetworkCredential credential, ImpersonationLogonType logonType, Func<T> function)
        {
            var context = GetContext(credential, logonType);

            try
            {
                return function();
            }
            finally
            {
                if (context != null)
                {
                    context.Undo();
                    context.Dispose();
                }
            }
        }

        private static WindowsImpersonationContext GetContext(NetworkCredential credential, ImpersonationLogonType logonType)
        {
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
                        (int)logonType,
                        Constants.LOGON32_PROVIDER_DEFAULT,
                        ref token) != 0)
                    {
                        if (NativeMethods.DuplicateToken(token, Constants.SECURITY_IMPERSONATION, ref tokenDuplicate) != 0)
                        {
                            return new WindowsIdentity(tokenDuplicate).Impersonate();
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
        #endregion
    }
}
