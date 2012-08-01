namespace IronFoundry.Misc.WinService
{
    using System;
    using System.Runtime.InteropServices;
    using IronFoundry.Misc.Logging;

    internal static class ServiceStatusUtil
    {
        [Flags]
        private enum SERVICE_STATE : int
        {
            SERVICE_STOPPED          = 0x00000001,
            SERVICE_START_PENDING    = 0x00000002,
            SERVICE_STOP_PENDING     = 0x00000003,
            SERVICE_RUNNING          = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING    = 0x00000006,
            SERVICE_PAUSED           = 0x00000007,
        }

        [Flags]
        private enum SERVICE_TYPES : int
        {
            SERVICE_KERNEL_DRIVER       = 0x00000001,
            SERVICE_FILE_SYSTEM_DRIVER  = 0x00000002,
            SERVICE_WIN32_OWN_PROCESS   = 0x00000010,
            SERVICE_WIN32_SHARE_PROCESS = 0x00000020,
            SERVICE_INTERACTIVE_PROCESS = 0x00000100
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SERVICE_STATUS 
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(SERVICE_STATUS));
            public SERVICE_TYPES dwServiceType;
            public SERVICE_STATE dwCurrentState;  
            public uint dwControlsAccepted;  
            public uint dwWin32ExitCode;  
            public uint dwServiceSpecificExitCode;  
            public uint dwCheckPoint;  
            public uint dwWaitHint;
        } 

        [DllImport("advapi32.dll")]
        private static extern bool SetServiceStatus(IntPtr hServiceStatus, ref SERVICE_STATUS lpServiceStatus);

        public const uint ERROR_SERVICE_SPECIFIC_ERROR = 0x0000042a; // 1066

        public static void ErrorExit(ILog log, IntPtr serviceHandle, UInt32 serviceSpecificExitCode)
        {
            var status = new SERVICE_STATUS
            {
                dwServiceType             = SERVICE_TYPES.SERVICE_WIN32_OWN_PROCESS,
                dwCurrentState            = SERVICE_STATE.SERVICE_STOPPED,
                dwControlsAccepted        = 0,
                dwWin32ExitCode           = ERROR_SERVICE_SPECIFIC_ERROR,
                dwServiceSpecificExitCode = serviceSpecificExitCode,
                dwCheckPoint              = 0,
                dwWaitHint                = 5000,
            };
            bool serviceStatusRV = SetServiceStatus(serviceHandle, ref status);
            log.Debug("SetServiceStatus rv: {0}", serviceStatusRV);
        }
    }
}