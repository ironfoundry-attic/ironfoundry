namespace CloudFoundry.Net.Extensions
{
    using System;

    public static class StringExtensionMethods
    {
        public static bool IsNullOrWhiteSpace(this string argThis)
        {
            return String.IsNullOrWhiteSpace(argThis);
        }

        public static bool IsNullOrEmpty(this string argThis)
        {
            return String.IsNullOrEmpty(argThis);
        }
    }
}