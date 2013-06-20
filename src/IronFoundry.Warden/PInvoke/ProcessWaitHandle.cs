namespace IronFoundry.Warden.PInvoke
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class ProcessWaitHandle : WaitHandle
    {
        private static readonly NativeMethods.DuplicateOptions handleOptions = NativeMethods.DuplicateOptions.DUPLICATE_SAME_ACCESS;

        public ProcessWaitHandle(SafeProcessHandle processHandle)
        {
            IntPtr waitHandle = IntPtr.Zero;

            if (!NativeMethods.DuplicateHandle(
                Utils.GetCurrentProcessRef(this),
                processHandle,
                Utils.GetCurrentProcessRef(this), out waitHandle, 0, false, handleOptions))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            base.SafeWaitHandle = new SafeWaitHandle(waitHandle, true);
        }
    }
}
