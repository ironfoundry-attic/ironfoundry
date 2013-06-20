namespace IronFoundry.Warden.PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    internal partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public class StartupInfo : IDisposable
        {
            public Int32 cb              = 0;
            public IntPtr lpReserved     = IntPtr.Zero;
            public String lpDesktop = String.Empty; // NB: if things don't work, try IntPtr.Zero
            public IntPtr lpTitle        = IntPtr.Zero;
            public Int32 dwX             = 0;
            public Int32 dwY             = 0;
            public Int32 dwXSize         = 0;
            public Int32 dwYSize         = 0;
            public Int32 dwXCountChars   = 0;
            public Int32 dwYCountChars   = 0;
            public Int32 dwFillAttribute = 0;
            public Int32 dwFlags         = 0;
            public Int16 wShowWindow     = 0;
            public Int16 cbReserved2     = 0;
            public IntPtr lpReserved2    = IntPtr.Zero;
            public SafeFileHandle hStdInput;
            public SafeFileHandle hStdOutput;
            public SafeFileHandle hStdError;

            public StartupInfo()
            {
                this.cb = Marshal.SizeOf(this);
            }

            public void Dispose()
        	{
        		if (this.hStdInput != null && !this.hStdInput.IsInvalid)
        		{
        			this.hStdInput.Close();
        			this.hStdInput = null;
        		}
        		if (this.hStdOutput != null && !this.hStdOutput.IsInvalid)
        		{
        			this.hStdOutput.Close();
        			this.hStdOutput = null;
        		}
        		if (this.hStdError != null && !this.hStdError.IsInvalid)
        		{
        			this.hStdError.Close();
        			this.hStdError = null;
        		}
        	}
        }
    }
}
