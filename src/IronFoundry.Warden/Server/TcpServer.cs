namespace IronFoundry.Warden.Server
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Handlers;
    using IronFoundry.Warden.Jobs;
    using IronFoundry.Warden.Protocol;
    using IronFoundry.Warden.Utilities;
    using NLog;
    using ProtoBuf;

    public class TcpServer
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly CancellationToken cancellationToken;

        private readonly IContainerManager containerManager;
        private readonly IJobManager jobManager;

        public TcpServer(IContainerManager containerManager, IJobManager jobManager, CancellationToken cancellationToken)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            if (jobManager == null)
            {
                throw new ArgumentNullException("jobManager");
            }
            if (cancellationToken == null)
            {
                throw new ArgumentNullException("cancellationToken");
            }
            this.containerManager = containerManager;
            this.jobManager = jobManager;
            this.cancellationToken = cancellationToken;
        }

        public async void RunServer()
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Statics.OnServiceStart();

            var endpoint = new IPEndPoint(IPAddress.Loopback, 4444); // TODO configurable port
            var listener = new TcpListener(endpoint); // lib/dea/task.rb, 66
            listener.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                log.Trace("Client connected!");
                ProcessClient(client);
            }

            log.Debug("Stopping Server.");
            listener.Stop();
        }

        public async void ProcessClient(TcpClient client)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            log.Trace("ProcessClient START {0}", client.GetHashCode());
            uint messageCount = 0;

            using (client)
            {
                using (var buffer = new Buffer())
                {
                    using (NetworkStream ns = client.GetStream())
                    {
                        var byteBuffer = new byte[client.ReceiveBufferSize];
                        while (client.Connected && !cancellationToken.IsCancellationRequested) // TODO: graceful cancel
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
                                while (ns.DataAvailable && !cancellationToken.IsCancellationRequested);

                                foreach (var request in buffer.GetMessages())
                                {
                                    if (!client.Connected)
                                    {
                                        break;
                                    }

                                    HandleRequest(request, response =>
                                        {
                                            if (response != null)
                                            {
                                                ++messageCount;
                                            }
                                            WriteMessage(response, ns);
                                        });
                                }
                            }
                            catch (Exception ex)
                            {
                                HandleSocketException(ex);
                                ErrorResponse(ex as WardenException, ns);
                            }
                        }
                    }
                }

                client.Close();
            }

            log.Trace("ProcessClient STOP Message count: '{0}'", messageCount);
        }

        private void ErrorResponse(WardenException ex, NetworkStream ns)
        {
            if (ex != null)
            {
                var response = new ErrorResponse { Message = ex.ResponseMessage, Data = ex.StackTrace };
                var wrapper = new ResponseWrapper(response);
                Message errorMessage = wrapper.GetMessage();
                WriteMessage(errorMessage, ns);
            }
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
                DoWriteMessage(ns, payloadLenBytes, responsePayload);
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

        private void HandleRequest(Message msg, Action<Message> responseWriter)
        {
            log.Trace("HandleRequest: '{0}'", msg.MessageType.ToString());

            var unwrapper = new MessageUnwrapper(msg);
            Request request = unwrapper.GetRequest();

            var factory = new RequestHandlerFactory(containerManager, jobManager, msg.MessageType, request);
            RequestHandler handler = factory.GetHandler();

            Response response = null;
            try
            {
                var streamingHandler = handler as IStreamingHandler;
                if (streamingHandler != null)
                {
                    while (!(streamingHandler.Complete || cancellationToken.IsCancellationRequested))
                    {
                        response = handler.Handle();
                        var wrapper = new ResponseWrapper(response);
                        responseWriter(wrapper.GetMessage());
                    }
                }
                else
                {
                    response = handler.Handle();
                }
            }
            catch (Exception ex)
            {
                if (ex is WardenException)
                {
                    throw;
                }
                else
                {
                    throw new WardenException(
                        String.Format("Exception in request handler '{0}'", handler.ToString()), ex);
                }
            }

            var lastWrapper = new ResponseWrapper(response);
            responseWriter(lastWrapper.GetMessage());
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
