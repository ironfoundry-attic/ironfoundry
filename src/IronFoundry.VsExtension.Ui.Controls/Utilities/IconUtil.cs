namespace IronFoundry.VsExtension.Ui.Controls.Utilities
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
            RegistryKey currentUser = Registry.CurrentUser;
            var className = "Unknown";
            var classKey = currentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\"+extension+@"\OpenWithProgids");
            if (classKey != null)
                className = classKey.GetValueNames().First();
            RegistryKey Root = Registry.ClassesRoot;
            if (className.EndsWith("Folder"))
                className = "Folder";
            if (className.Equals("icofile"))
                className = "Unknown";

            RegistryKey ApplicationKey = Root.OpenSubKey(className);

            RegistryKey CurrentVer = null;
            try
            {
                CurrentVer = Root.OpenSubKey(ApplicationKey.OpenSubKey("CurVer").GetValue("").ToString());
            }
            catch (Exception) { }

            if (CurrentVer != null)
                ApplicationKey = CurrentVer;

            if (ApplicationKey == null)
                ApplicationKey = Root.OpenSubKey("Unknown");

            string IconLocation =
                ApplicationKey.OpenSubKey("DefaultIcon").GetValue("").ToString();
            string[] IconPath = IconLocation.Split(',');


            IntPtr[] Large = null;
            IntPtr[] Small = null;
            int iIconPathNumber = 0;

            if (IconPath.Length > 1)
                iIconPathNumber = 1;
            else
                iIconPathNumber = 0;


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