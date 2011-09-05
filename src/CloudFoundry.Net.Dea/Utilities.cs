namespace CloudFoundry.Net.Dea
{
    using System;
    using System.IO;
    using System.Web.Script.Serialization;

    public static class Utility
    {
        private static readonly JavaScriptSerializer jsonSerializer;
        
        static Utility() 
        {
            jsonSerializer = new JavaScriptSerializer();
        }

        public static string ToJson(this object obj)
        {
            return jsonSerializer.Serialize(obj);
        }  
      
        public static T FromJson<T>(this string jsonString)
        {
            return jsonSerializer.Deserialize<T>(jsonString);
        }     
   
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
}