using System;

namespace IronFoundry.Extensions
{
    public static class UriExtensionMethods
    {
        private static readonly char[] trimChars = new char[] { '/' };

        public static string AbsoluteUriTrimmed(this Uri argThis)
        {
            return argThis.AbsoluteUri.TrimEnd(trimChars);
        }
    }
}