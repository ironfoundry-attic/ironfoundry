namespace IronFoundry.Warden.Server
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Jobs;
    using IronFoundry.Warden.Protocol;
    using IronFoundry.Warden.Utilities;
    using NLog;

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
                await ProcessClientAsync(client);
            }

            log.Debug("Stopping Server.");
            listener.Stop();
        }

        public async Task ProcessClientAsync(TcpClient client)
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

                                foreach (Message message in buffer.GetMessages())
                                {
                                    if (!client.Connected)
                                    {
                                        break;
                                    }

                                    var messageWriter = new MessageWriter(ns);
                                    var messageHandler = new MessageHandler(containerManager, jobManager, cancellationToken, messageWriter);
                                    await messageHandler.HandleAsync(message);
                                    log.Trace("Finished handling message: '{0}'", message.MessageType.ToString());
                                }
                            }
                            catch (Exception exception)
                            {
                                var socketExceptionHandler = new SocketExceptionHandler(exception);
                                if (!socketExceptionHandler.Handle())
                                {
                                    var messageWriter = new MessageWriter(ns);
                                    var wardenExceptionHandler = new WardenExceptionHandler(exception, messageWriter);
                                    if (!wardenExceptionHandler.Handle())
                                    {
                                        log.ErrorException(exception);
                                    }
                                }
                            }
                        }
                    }
                }

                client.Close();
            }

            log.Trace("ProcessClient STOP Message count: '{0}'", messageCount);
        }
    }
}
