namespace IronFoundry.Warden.PInvoke
{
    using System;
    using System.Runtime.InteropServices;

    internal partial class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();
    }
}
