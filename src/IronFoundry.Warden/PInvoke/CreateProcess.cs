namespace IronFoundry.Warden.PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal partial class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CreateProcess(
            String lpApplicationName,
            [In] StringBuilder lpCommandLine,
            SecurityAttributes lpProcessAttributes,
            SecurityAttributes lpThreadAttributes,
            Boolean bInheritHandles,
            CreateProcessFlags dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            [In] StartupInfo lpStartupInfo,
            out ProcessInformation lpProcessInformation);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean CreateProcessAsUser 
        (
            IntPtr hToken,
            String lpApplicationName,
            [In] StringBuilder lpCommandLine,
            SecurityAttributes lpProcessAttributes,
            SecurityAttributes lpThreadAttributes,
            Boolean bInheritHandles,
            CreateProcessFlags dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            [In] StartupInfo lpStartupInfo,
            out ProcessInformation lpProcessInformation
        );
    }
}
