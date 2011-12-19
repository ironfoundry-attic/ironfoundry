using System.Collections.Generic;
using System.Linq;

namespace CloudFoundry.Net.Extensions
{
    public static class IEnumerableExtensionMethods
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> argThis)
        {
            return null == argThis || false == argThis.Any();
        }

        public static IList<T> ToListOrNull<T>(this IEnumerable<T> argThis)
        {
            if (null == argThis)
                return null;

            return argThis.ToList();
        }

        public static T[] ToArrayOrNull<T>(this IEnumerable<T> argThis)
        {
            if (null == argThis)
                return null;

            return argThis.ToArray();
        }
    }
}