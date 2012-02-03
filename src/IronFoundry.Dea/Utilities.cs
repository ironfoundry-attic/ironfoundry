namespace IronFoundry.Dea
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using Microsoft.VisualBasic.Devices;

    public static class Utility
    {
        private static readonly IPAddress[] localAddresses;

        static Utility()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            localAddresses = host.AddressList;
        }

        public static int GetEpochTimestamp()
        {
            return (int)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            var c = new Computer();
            c.FileSystem.CopyDirectory(source.FullName, target.FullName, true);
        }

        public static string GetFileSizeString(long size)
        {
            var sizes = new[] { "B", "K", "M", "G" };
            var decimalSize = Convert.ToDecimal(size);
            var index = 0;
            while (size >= 1024 && index++ < sizes.Length)
            {
                size = size / 1024;
                decimalSize = decimalSize / 1024m;
            }

            return string.Format("{0:0.##}{1}", decimalSize, sizes[index]);
        }

        public static ushort RandomFreePort()
        {
            var socket = new TcpListener(IPAddress.Any, 0);
            socket.Start();
            ushort rv = Convert.ToUInt16(((IPEndPoint)socket.LocalEndpoint).Port);
            socket.Stop();
            return rv;
        }

        public static bool IsLocalIpAddress(string host)
        {
            try
            {
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                foreach (IPAddress hostIP in hostIPs)
                {
                    if (IPAddress.IsLoopback(hostIP))
                    {
                        return true;
                    }
                    foreach (IPAddress localIP in localAddresses)
                    {
                        if (hostIP.Equals(localIP))
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }
    }
}