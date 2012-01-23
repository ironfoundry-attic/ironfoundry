namespace System.Net.Sockets
{
    public static class TcpClientExtensionMethods
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