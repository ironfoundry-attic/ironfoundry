namespace IronFoundry.Bosh.Agent
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    public static class EjectMedia
    {
        const uint GENERIC_READ              = 0x80000000;
        const uint GENERIC_WRITE             = 0x40000000;
        const uint OPEN_EXISTING             = 3;
        const uint IOCTL_STORAGE_EJECT_MEDIA = 2967560;
        const int  INVALID_HANDLE            = -1;

        private static readonly Regex driveLetterRe = new Regex(@"^[A-Z]:$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static IntPtr fileHandle;
        private static uint returnedBytes;

        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr attributes,
            uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile); 

        [DllImport("kernel32", SetLastError = true)]
        static extern int CloseHandle(IntPtr driveHandle);

        [DllImport("kernel32", SetLastError = true)]
        static extern bool DeviceIoControl(IntPtr driveHandle, uint IoControlCode, IntPtr lpInBuffer, uint inBufferSize,
            IntPtr lpOutBuffer, uint outBufferSize, ref uint lpBytesReturned, IntPtr lpOverlapped);

        public static void Eject(string driveLetter)
        {
            if (driveLetterRe.IsMatch(driveLetter))
            {
                try
                {
                    // Create an handle to the drive
                    fileHandle = CreateFile(@"\\.\" + driveLetter, GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                    if ((int)fileHandle != INVALID_HANDLE)
                    {
                        // Eject the disk
                        DeviceIoControl(fileHandle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, ref returnedBytes, IntPtr.Zero);
                    }
                }
                catch
                {
                    throw new Exception(Marshal.GetLastWin32Error().ToString());
                }
                finally
                {
                    // Close Drive Handle
                    CloseHandle(fileHandle);
                    fileHandle = IntPtr.Zero;
                }
            }
            else
            {
                throw new ArgumentException("Invalid drive letter!", "driveLetter");
            }
        }
    }
}