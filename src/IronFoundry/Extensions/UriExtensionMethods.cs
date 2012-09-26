using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Extensions
{
    public static class UriExtensionMethods
    {
        private static readonly char[] TrimChars = new char[] { '/' };

        public static string AbsoluteUriTrimmed(this Uri argThis)
        {
            return argThis.AbsoluteUri.TrimEnd(TrimChars);
        }
    }
}
