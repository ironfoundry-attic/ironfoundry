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
    internal static class IEnumerableExtensionMethods
    {
        public static bool IsNullOrEmpty(this IEnumerable argThis)
        {
            return null == argThis || false == argThis.GetEnumerator().MoveNext();
        }
    }
}

namespace System.Collections.Generic
{
    using System.Linq;

    internal static class IEnumerableExtensionMethods
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

        public static IEnumerable<T> Compact<T>(this IEnumerable<T> argThis)
        {
            if (null == argThis)
                return null;

            return argThis.Where<T>(t => t != null);
        }
    }
}

namespace System.Text.RegularExpressions
{
    internal static class RegexExtensionMethods
    {
        public static string Postmatch(this Match match, string target)
        {
            int unmatchedIdx = match.Index + match.Length;
            return target.Substring(unmatchedIdx);
        }
    }
}

namespace System.Net.Sockets
{
    internal static class TcpClientExtensionMethods
    {
        public static int Read(this TcpClient client, byte[] buffer)
        {
            NetworkStream stream = client.GetStream();
            return stream.Read(buffer, 0, buffer.Length);
        }

        public static void Write(this TcpClient client, byte[] data)
        {
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }

        public static bool DataAvailable(this TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            return stream.DataAvailable;
        }

        public static void CloseStream(this TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            stream.Close();
            stream.Dispose();
        }
    }
}

namespace System.Threading
{
    using System;

    public static class TimerExtensionMethods
    {
        public static void Stop(this Timer argThis)
        {
            argThis.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public static void Restart(this Timer argThis, TimeSpan argInterval)
        {
            argThis.Change(argInterval, argInterval);
        }
    }
}