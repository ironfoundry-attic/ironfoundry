using System;
using System.Threading;
using IronFoundry.Warden.Containers;
using IronFoundry.Warden.Handlers;
using IronFoundry.Warden.Jobs;
using IronFoundry.Warden.Protocol;
using NLog;

namespace IronFoundry.Warden.Server
{
    public class MessageHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IContainerManager containerManager;
        private readonly IJobManager jobManager;
        private readonly CancellationToken cancellationToken;
        private readonly MessageWriter messageWriter;

        public MessageHandler(IContainerManager containerManager, IJobManager jobManager,
            CancellationToken cancellationToken, MessageWriter messageWriter)
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
            if (messageWriter == null)
            {
                throw new ArgumentNullException("messageWriter");
            }
            this.containerManager = containerManager;
            this.jobManager = jobManager;
            this.cancellationToken = cancellationToken;
            this.messageWriter = messageWriter;
        }

        public async void Handle(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            log.Trace("MessageType: '{0}'", message.MessageType.ToString());

            var unwrapper = new MessageUnwrapper(message);
            Request request = unwrapper.GetRequest();

            var factory = new RequestHandlerFactory(containerManager, jobManager, message.MessageType, request);
            RequestHandler handler = factory.GetHandler();

            try
            {
                var streamingHandler = handler as IStreamingHandler;
                if (streamingHandler != null)
                {
                    Response finalResponse = await streamingHandler.HandleAsync(messageWriter, cancellationToken);
                    if (finalResponse == null)
                    {
                        string errorMessage = String.Format("Null final response from streaming handler '{0}'", streamingHandler.GetType());
                        log.Error(errorMessage);
                        finalResponse = new ErrorResponse { Message = errorMessage };
                    }

                    messageWriter.Write(finalResponse);
                }
                else
                {
                    Response response = handler.Handle();
                    messageWriter.Write(response);
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
        }
    }
}
