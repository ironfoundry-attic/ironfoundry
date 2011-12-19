using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CloudFoundry.Net.Utilities;

namespace CloudFoundry.Net.Extensions
{
    public static class ObjectExtensionMethods
    {
        public static T DeepCopy<T>(this T obj)
        {
            object result = null;

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Binder = new CustomSerializationBinder();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                result = (T)formatter.Deserialize(ms);
                ms.Close();
            }

            return (T)result;
        }        
    }
}