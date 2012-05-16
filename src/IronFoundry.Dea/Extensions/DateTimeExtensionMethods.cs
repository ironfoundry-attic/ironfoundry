namespace System
{
    public static class DateTimeExtensionMethods
    {
        private const string JsonDateFormat = "yyyy-MM-dd HH:mm:ss zz00";

        public static string ToJsonString(this DateTime argThis)
        {
            return argThis.ToString(JsonDateFormat);
        }
    }
}