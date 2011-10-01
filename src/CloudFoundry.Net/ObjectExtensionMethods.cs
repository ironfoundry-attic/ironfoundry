namespace CloudFoundry.Net
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public static class ObjectExtensionMethods
    {
        public static T DeepCopy<T>(this T obj)
        {
            object result = null;

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                result = (T)formatter.Deserialize(ms);
                ms.Close();
            }

            return (T)result;
        }
    }
}