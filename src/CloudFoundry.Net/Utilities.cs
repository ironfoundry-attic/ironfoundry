namespace CloudFoundry.Net
{
    using System;
    using System.IO;

    public static class Utility
    {
        public static int GetEpochTimestamp()
        {
            return (int)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            if (!Directory.Exists(target.FullName))
                Directory.CreateDirectory(target.FullName);
            foreach (var file in source.GetFiles())
                file.CopyTo(Path.Combine(target.ToString(),file.Name),true);
            foreach (var directory in source.GetDirectories())
            {
                DirectoryInfo nextDirectory = target.CreateSubdirectory(directory.Name);
                CopyDirectory(directory,nextDirectory);
            }
        } 
    }
}