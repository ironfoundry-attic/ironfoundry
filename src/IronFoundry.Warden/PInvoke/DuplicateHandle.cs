namespace IronFoundry.Warden.PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    internal partial class NativeMethods
    {
        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool DuplicateHandle(HandleRef hSourceProcessHandle,
            SafeFileHandle sourceHandle, HandleRef hTargetProcess, out SafeFileHandle targetHandle, int dwDesiredAccess, bool bInheritHandle, DuplicateOptions dwOptions);

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool DuplicateHandle(HandleRef hSourceProcessHandle,
            SafeHandle sourceHandle, HandleRef hTargetProcess, out IntPtr targetHandle, int dwDesiredAccess, bool bInheritHandle, DuplicateOptions dwOptions);
    }
}
