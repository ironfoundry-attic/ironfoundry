namespace CloudFoundry.Net.Dea
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /*
     * TODO break out into common dll or other better place for ex methods
     */
    public static class Utility
    {
        /*
        public static string ToJson(this object obj)
        {
            return JsonConvert.ToString(obj);
        }  
      
        public static T FromJson<T>(this string argJson)
        {
            return JsonConvert.DeserializeObject<T>(argJson);
        }     
         */
   
        public static int GetEpochTimestamp()
        {
            return (int)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            if (!Directory.Exists(target.FullName))
                Directory.CreateDirectory(target.FullName);
            foreach(var file in source.GetFiles())
                file.CopyTo(Path.Combine(target.ToString(),file.Name),true);
            foreach(var directory in source.GetDirectories())
            {
                DirectoryInfo nextDirectory = target.CreateSubdirectory(directory.Name);
                CopyDirectory(directory,nextDirectory);
            }
        } 
    }

    public static class IEnumerableExtensionMethods
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> argThis)
        {
            return null == argThis || false == argThis.Any();
        }
    }
}