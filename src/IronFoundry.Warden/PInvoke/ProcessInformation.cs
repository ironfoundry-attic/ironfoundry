namespace IronFoundry.Warden.PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    internal partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessInformation
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public Int32 dwProcessId;
            public Int32 dwThreadId;
        }
    }
}
