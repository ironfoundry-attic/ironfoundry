namespace IronFoundry.Ui.Controls.Utilities
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;

    public static class IconUtil
    {               
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

        public static Icon IconFromExtension(string extension, SystemIconSize size)
        {            
            var className = "Unknown";
            var classKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\"+extension+@"\OpenWithProgids");
            if (classKey != null)
                className = classKey.GetValueNames().First();
            
            if (className.EndsWith("Folder"))
                className = "Folder";
            if (className.Equals("icofile"))
                className = "Unknown";

            Icon returnIcon = null;
            try
            {
                returnIcon = IconFromClassName(size, className);
            }
            catch
            {
                returnIcon = IconFromClassName(size, "Unknown");
            }
            return returnIcon;
        }

        private static Icon IconFromClassName(SystemIconSize size, string className)
        {
            var Root = Registry.ClassesRoot;
            var ApplicationKey = Root.OpenSubKey(className);
            RegistryKey CurrentVer = null;
            try
            {
                CurrentVer = Root.OpenSubKey(ApplicationKey.OpenSubKey("CurVer").GetValue("").ToString());
            }
            catch (Exception)
            {
            }

            if (CurrentVer != null)
                ApplicationKey = CurrentVer;

            if (ApplicationKey == null)
                ApplicationKey = Root.OpenSubKey("Unknown");

            var IconLocation =
                ApplicationKey.OpenSubKey("DefaultIcon").GetValue("").ToString();
            var IconPath = IconLocation.Split(',');
            IntPtr[] Large = null;
            IntPtr[] Small = null;
            var iIconPathNumber = 0;
            iIconPathNumber = IconPath.Length > 1 ? 1 : 0;
            if (IconPath[iIconPathNumber] == null) IconPath[iIconPathNumber] = "0";
            Large = new IntPtr[1];
            Small = new IntPtr[1];
            if (iIconPathNumber > 0)
                ExtractIconEx(IconPath[0], Convert.ToInt16(IconPath[iIconPathNumber]), Large, Small, 1);
            else
                ExtractIconEx(IconPath[0], Convert.ToInt16(0), Large, Small, 1);

            return size == SystemIconSize.Large ? Icon.FromHandle(Large[0]) : Icon.FromHandle(Small[0]);
        }
    }
}
