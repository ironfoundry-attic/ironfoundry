namespace System
{
    internal static class StringExtensionMethods
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

namespace System.Collections
{
    internal static class EnumerableExtensionMethods
    {
        public static bool IsNullOrEmpty(this IEnumerable argThis)
        {
            return null == argThis || false == argThis.GetEnumerator().MoveNext();
        }
    }
}

namespace System.Collections.Generic
{
    using Linq;

    internal static class EnumerableExtensionMethods
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