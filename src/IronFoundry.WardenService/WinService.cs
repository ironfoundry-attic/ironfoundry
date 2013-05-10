namespace IronFoundry.WardenService
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Warden;
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
                log.Trace("Client connected!");
                ProcessClient(client);
            }

            log.Debug("Stopping Server.");
            listener.Stop();
        }

        private async void ProcessClient(TcpClient client)
        {
            log.Trace("ProcessClient START {0}", client.GetHashCode());
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
                                    if (bytes == 0)
                                    {
                                        // Still there?
                                        ns.Write(Constants.CRLF, 0, 2); 
                                    }
                                    else
                                    {
                                        buffer.Push(byteBuffer, bytes);
                                    }
                                }
                                while (ns.DataAvailable && !ct.IsCancellationRequested);

                                foreach (var request in buffer.GetMessages())
                                {
                                    if (!client.Connected)
                                    {
                                        break;
                                    }

                                    Message response = HandleRequest(request);
                                    if (response != null)
                                    {
                                        ++messageCount;
                                    }

                                    if (!client.Connected)
                                    {
                                        break;
                                    }

                                    WriteMessage(response, ns);
                                }
                            }
                            catch (Exception ex)
                            {
                                HandleSocketException(ex);
                                if (ex is WardenException)
                                {
                                    ErrorResponse(ex, ns);
                                }
                                break;
                            }
                        }
                    }
                }

                client.Close();
            }

            log.Trace("ProcessClient STOP Message count: '{0}'", messageCount);
        }

        private void ErrorResponse(Exception ex, NetworkStream ns)
        {
            var response = new ErrorResponse { Message = ex.Message, Data = ex.StackTrace };
            var wrapper = new ResponseWrapper(response);
            Message errorMessage = wrapper.GetMessage();
            WriteMessage(errorMessage, ns);
        }

        private void WriteMessage(Message rsp, NetworkStream ns)
        {
            byte[] responsePayload = null;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, rsp);
                responsePayload = ms.ToArray();
            }

            int payloadLen = responsePayload.Length;
            var payloadLenBytes = Encoding.ASCII.GetBytes(payloadLen.ToString());

            try
            {
#if DEBUG
                // TODO: MemoryStream/ToArray for debugging only
                byte[] toWrite = null;
                using (var ms = new MemoryStream())
                {
                    DoWriteMessage(ms, payloadLenBytes, responsePayload);
                    toWrite = ms.ToArray();
                }
                Debug.WriteLine(String.Format("MESSAGE: {0}", BitConverter.ToString(toWrite)));
                ns.Write(toWrite, 0, toWrite.Length);
#else
                DoWriteMessage(ns, payloadLenBytes, responsePayload);
#endif
            }
            catch (Exception ex)
            {
                HandleSocketException(ex);
            }
        }

        private static void DoWriteMessage(Stream s, byte[] payloadLenBytes, byte[] responsePayload)
        {
            s.Write(payloadLenBytes, 0, payloadLenBytes.Length);
            s.WriteByte(Constants.CR);
            s.WriteByte(Constants.LF);
            s.Write(responsePayload, 0, responsePayload.Length);
            s.WriteByte(Constants.CR);
            s.WriteByte(Constants.LF);
        }

        private Message HandleRequest(Message msg)
        {
            log.Trace("HandleRequest: '{0}'", msg.MessageType.ToString());

            var unwrapper = new MessageUnwrapper(msg);
            Request request = unwrapper.GetRequest();

            var factory = new RequestHandlerFactory(msg.MessageType, request);
            var handler = factory.GetHandler();
            Response response = handler.Handle();

            var wrapper = new ResponseWrapper(response);
            return wrapper.GetMessage();
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
