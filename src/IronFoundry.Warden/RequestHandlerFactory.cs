namespace IronFoundry.Warden
{
    using System;
    using IronFoundry.WardenProtocol;

    public class RequestHandlerFactory
    {
        private readonly Message.Type requestType;
        private readonly Request request;

        public RequestHandlerFactory(Message.Type requestType, Request request)
        {
            if (requestType == default(Message.Type))
            {
                throw new ArgumentNullException("requestType");
            }
            if (request == null)
            {
                throw new ArgumentNullException("message");
            }
            this.requestType = requestType;
            this.request = request;
        }

        public RequestHandler GetHandler()
        {
            RequestHandler handler = null;

            switch (requestType)
            {
                case Message.Type.Ping:
                    handler = new PingRequestHandler(request);
                    break;
                case Message.Type.Echo:
                    handler = new EchoRequestHandler(request);
                    break;
            }

            return handler;
        }
    }
}
