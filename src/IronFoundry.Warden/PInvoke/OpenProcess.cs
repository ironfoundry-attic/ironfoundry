namespace IronFoundry.Warden.PInvoke
{
    using System.Runtime.InteropServices;

    internal partial class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeProcessHandle OpenProcess(int access, bool inherit, int processId);
    }
}
