namespace IronFoundry.Warden.PInvoke
{
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    internal partial class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreatePipe(
            out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe,
            NativeMethods.SecurityAttributes lpPipeAttributes, int nSize);
    }
}
