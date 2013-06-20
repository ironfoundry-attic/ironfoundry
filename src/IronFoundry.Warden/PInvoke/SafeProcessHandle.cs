namespace IronFoundry.Warden.PInvoke
{
    using System;
    using Microsoft.Win32.SafeHandles;

    internal sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public static SafeProcessHandle InvalidHandle = new SafeProcessHandle(IntPtr.Zero);

        public SafeProcessHandle() : base(true)
        {
        }

        public SafeProcessHandle(IntPtr existingHandle) : base(true)
        {
            base.SetHandle(existingHandle);
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(this.handle);
        }
    }
}
