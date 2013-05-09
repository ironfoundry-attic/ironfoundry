namespace IronFoundry.Warden
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.WardenProtocol;
    using NLog;
    using ProtoBuf;
    using Topshelf;

    public class WinService : ServiceControl
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly Task listenerTask;

        public WinService()
        {
            ct = cts.Token;
            listenerTask = new Task(WardenServer, ct);
        }

        public bool Start(HostControl hostControl)
        {
            listenerTask.Start();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            try
            {
                cts.Cancel();
                Task.WaitAll(new[] { listenerTask }, (int)TimeSpan.FromSeconds(25).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                log.InfoException(ex.Message, ex);
            }
            return true;
        }

        private async void WardenServer()
        {
            if (cts.IsCancellationRequested)
            {
                return;
            }

            var endpoint = new IPEndPoint(IPAddress.Loopback, 4444);
            var listener = new TcpListener(endpoint); // lib/dea/task.rb, 66
            listener.Start();

            while (!cts.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                log.Debug("Client connected!");
                ProcessClient(client);
            }

            log.Debug("Stopping Server.");
            listener.Stop();
        }

        private async void ProcessClient(TcpClient client)
        {
            log.Debug("ProcessClient START {0}", client.GetHashCode());
            uint messageCount = 0;
            using (client)
            {
                using (var buffer = new WBuffer())
                {
                    using (NetworkStream ns = client.GetStream())
                    {
                        var byteBuffer = new byte[client.ReceiveBufferSize];
                        while (client.Connected && !ct.IsCancellationRequested) // TODO: graceful cancel
                        {
                            try
                            {
                                do
                                {
                                    int bytes = await ns.ReadAsync(byteBuffer, 0, byteBuffer.Length);
                                    if (bytes > 0)
                                    {
                                        buffer.Push(byteBuffer, bytes);
                                    }
                                }
                                while (ns.DataAvailable && !ct.IsCancellationRequested);

                                foreach (var request in buffer.GetMessages())
                                {
                                    ++messageCount;

                                    Message rsp = HandleRequest(request);

                                    // TODO: buffer.rb / payload_to_wire
                                    byte[] rspData = null;
                                    using (var ms = new MemoryStream())
                                    {
                                        Serializer.Serialize(ms, rsp);
                                        rspData = ms.ToArray();
                                    }

                                    if (client.Connected)
                                    {
                                        try
                                        {
                                            client.Write(rspData);
                                        }
                                        catch (Exception ex)
                                        {
                                            HandleSocketException(ex);
                                        }
                                    }
                                    else
                                    {
                                        // TODO?
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                HandleSocketException(ex);
                            }
                        }
                    }

                    foreach (var request in buffer.GetMessages())
                    {
                        ++messageCount;
                        HandleRequest(request);
                    }
                }

                client.Close();
            }

            log.Debug("ProcessClient STOP Message count: '{0}'", messageCount);
        }

        private Message HandleRequest(Message msg)
        {
            log.Trace("HandleRequest: '{0}'", msg.type.ToString());

            Message response = null;

            switch (msg.type)
            {
                case Message.Type.Ping:
                    var rsp = new PingResponse();
                    byte[] msgPayload = null;
                    using (var ms = new MemoryStream())
                    {
                        Serializer.Serialize(ms, rsp);
                        msgPayload = ms.ToArray();
                    }
                    response = new Message { type = Message.Type.Ping, payload = msgPayload };
                    break;
            }

            return response;
        }

        private void HandleSocketException(Exception ex)
        {
            var ioException = ex as IOException;
            if (ioException != null)
            {
                bool handled = false;
                var socketException = ex.InnerException as SocketException;
                if (socketException != null)
                {
                    switch (socketException.SocketErrorCode)
                    {
                        case SocketError.ConnectionAborted:
                        case SocketError.ConnectionReset:
                            handled = true;
                            break;
                    }
                }
                if (!handled)
                {
                    log.ErrorException(ex.Message, ex);
                }
            }
            else
            {
                log.ErrorException(ex.Message, ex);
            }
        }
    }
}
