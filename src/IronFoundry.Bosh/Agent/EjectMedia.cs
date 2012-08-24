namespace IronFoundry.Bosh.Agent
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// http://support.microsoft.com/kb/q165721
    /// </summary>
    public static class EjectMedia
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct PREVENT_MEDIA_REMOVAL
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool PreventMediaRemoval;
        }

        private const ushort FILE_ANY_ACCESS     = 0;
        private const ushort FILE_SPECIAL_ACCESS = FILE_ANY_ACCESS;
        private const ushort FILE_READ_ACCESS    = 0x0001;
        private const ushort FILE_WRITE_ACCESS   = 0x0002;

        private const ushort METHOD_BUFFERED   = 0;
        private const ushort METHOD_IN_DIRECT  = 1;
        private const ushort METHOD_OUT_DIRECT = 2;
        private const ushort METHOD_NEITHER    = 3;

        private static uint CTL_CODE(ushort deviceType, ushort function, ushort method, ushort access)
        {
            /*
            #define CTL_CODE( DeviceType, Function, Method, Access ) (((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method))
             */
            return (uint)(((deviceType) << 16) | ((access) << 14) | ((function) << 2) | (method));
        }

        private const ushort FILE_DEVICE_MASS_STORAGE = 0x0000002d;
        private const ushort IOCTL_STORAGE_BASE = FILE_DEVICE_MASS_STORAGE;
        private const ushort FILE_DEVICE_FILE_SYSTEM = 0x00000009;

        private static readonly uint FSCTL_LOCK_VOLUME = CTL_CODE(FILE_DEVICE_FILE_SYSTEM,  6, METHOD_BUFFERED, FILE_ANY_ACCESS);
        private static readonly uint FSCTL_DISMOUNT_VOLUME = CTL_CODE(FILE_DEVICE_FILE_SYSTEM, 8, METHOD_BUFFERED, FILE_ANY_ACCESS);
        
        private static readonly uint IOCTL_STORAGE_MEDIA_REMOVAL = CTL_CODE(IOCTL_STORAGE_BASE, 0x0201, METHOD_BUFFERED, FILE_READ_ACCESS);
        private static readonly uint IOCTL_STORAGE_EJECT_MEDIA = CTL_CODE(IOCTL_STORAGE_BASE, 0x0202, METHOD_BUFFERED, FILE_READ_ACCESS);

        private const uint GENERIC_READ     = 0x80000000;
        private const uint GENERIC_WRITE    = 0x40000000;

        private const uint FILE_SHARE_READ  = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;

        private const uint OPEN_EXISTING    = 3;

        private const string createFileNamePrefix = @"\\.\";
        private static readonly char[] driveLetterTrimChars = new[] { '\\' };
        private static readonly Regex driveLetterRe = new Regex(@"^[A-Z]:\\?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        [DllImport("kernel32", SetLastError = true)]
        static extern SafeFileHandle CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr attributes,
            uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile); 

        [DllImport("kernel32", SetLastError = true)]
        static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode, IntPtr lpInBuffer, int nInBufferSize,
            IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = true)]
        extern static bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode,
            ref PREVENT_MEDIA_REMOVAL lpInBuffer, int nInBufferSize,
            IntPtr lpOutBuffer, uint nOutBufferSize, ref uint lpBytesReturned, IntPtr lpOverlapped);

        public static void Eject(string driveLetter)
        {
            if (driveLetterRe.IsMatch(driveLetter))
            {
                uint returnedBytes = 0;
                SafeFileHandle fileHandle = null;
                try
                {
                    string driveFileName = createFileNamePrefix + driveLetter.TrimEnd(driveLetterTrimChars);
                    fileHandle = CreateFile(driveFileName, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                    if (fileHandle.IsInvalid)
                    {
                        throw new Win32Exception();
                    }
                    // LOCK VOLUME
                    bool locked = false;
                    for (int i = 0; i < 20; ++i)
                    {
                        if (DeviceIoControl(fileHandle, FSCTL_LOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out returnedBytes, IntPtr.Zero))
                        {
                            locked = true;
                            break;
                        }
                        else
                        {
                            Thread.Sleep(500);
                        }
                    }
                    if (false == locked)
                    {
                        throw new Win32Exception();
                    }
                    // DISMOUNT VOLUME
                    if (false == DeviceIoControl(fileHandle, FSCTL_DISMOUNT_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out returnedBytes, IntPtr.Zero))
                    {
                        throw new Win32Exception();
                    }
                    // ENABLE REMOVAL
                    var pmr = new PREVENT_MEDIA_REMOVAL { PreventMediaRemoval = false };
                    int pmrSz = Marshal.SizeOf(pmr);
                    if (false == DeviceIoControl(fileHandle, IOCTL_STORAGE_MEDIA_REMOVAL, ref pmr, pmrSz, IntPtr.Zero, 0, ref returnedBytes, IntPtr.Zero))
                    {
                        throw new Win32Exception();
                    }
                    // AUTO EJECT
                    if (false == DeviceIoControl(fileHandle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, out returnedBytes, IntPtr.Zero))
                    {
                        throw new Win32Exception();
                    }
                }
                finally
                {
                    if (null != fileHandle)
                    {
                        fileHandle.Close();
                    }
                }
            }
            else
            {
                throw new ArgumentException("Invalid drive letter!", "driveLetter");
            }
        }
    }
}