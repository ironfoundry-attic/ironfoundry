namespace IronFoundry.Ui.Controls.Utilities
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;

    public static class IconUtil
    {               
        private const string DirectoryExtension = "Directory";

        [DllImport("Shell32", CharSet = CharSet.Auto)]
        extern static int ExtractIconEx(
            [MarshalAs(UnmanagedType.LPTStr)] 
            string lpszFile,
            int nIconIndex,
            IntPtr[] phIconLarge,
            IntPtr[] phIconSmall,
            int nIcons);

        public enum SystemIconSize : int
        {
            Large = 0x000000000,
            Small = 0x000000001
        }

        public static Icon DirectoryIcon(SystemIconSize size)
        {
            return IconFromExtension(DirectoryExtension, size);
        }

        public static Icon IconFromExtension(string extension, SystemIconSize size)
        {            
            Icon rv = null;

            string className = "Unknown";
            RegistryKey classKey = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + extension + @"\OpenWithProgids");
            if (null != classKey)
            {
                className = classKey.GetValueNames().First();
            }

            if (className.EndsWith("Folder") || extension == DirectoryExtension)
            {
                className = "Folder";
            }

            if (className.Equals("icofile"))
            {
                className = "Unknown";
            }

            try
            {
                rv = IconFromClassName(size, className);
            }
            catch { }

            if (null == rv)
            {
                rv = IconFromClassName(size, "Unknown");
            }

            return rv;
        }

        private static Icon IconFromClassName(SystemIconSize size, string className)
        {
            Icon rv = null;

            var root = Registry.ClassesRoot;
            var applicationKey = root.OpenSubKey(className);
            RegistryKey currentVer = null;

            RegistryKey curVerSubKey = applicationKey.OpenSubKey("CurVer");
            if (null != curVerSubKey)
            {
                object curVerValue = curVerSubKey.GetValue(String.Empty);
                if (null != curVerValue)
                {
                    currentVer = root.OpenSubKey(curVerValue.ToString());
                }
            }

            if (null != currentVer)
            {
                applicationKey = currentVer;
            }

            if (null == applicationKey)
            {
                applicationKey = root.OpenSubKey("Unknown");
            }

            RegistryKey appKeySubKey = applicationKey.OpenSubKey("DefaultIcon");
            string iconLocation = null;
            if (null != appKeySubKey)
            {
                object appKeySubKeyValue = appKeySubKey.GetValue(String.Empty);
                if (null != appKeySubKeyValue)
                {
                    iconLocation = appKeySubKeyValue.ToString();
                }
            }

            if (null != iconLocation)
            {
                string[] iconPath = iconLocation.Split(',');
                int iIconPathNumber = iconPath.Length > 1 ? 1 : 0;
                if (null == iconPath[iIconPathNumber])
                {
                    iconPath[iIconPathNumber] = "0";
                }

                IntPtr[] large = new IntPtr[1];
                IntPtr[] small = new IntPtr[1];
                if (iIconPathNumber > 0)
                {
                    ExtractIconEx(iconPath[0], Convert.ToInt16(iconPath[iIconPathNumber]), large, small, 1);
                }
                else
                {
                    ExtractIconEx(iconPath[0], Convert.ToInt16(0), large, small, 1);
                }

                rv = size == SystemIconSize.Large ? Icon.FromHandle(large[0]) : Icon.FromHandle(small[0]);
            }

            return rv;
        }
    }
}
