namespace IronFoundry.Warden.Handlers
{
    using System;
    using IronFoundry.Warden.Protocol;

    public abstract class RequestHandler
    {
        private readonly Request request;

        public RequestHandler(Request request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            this.request = request;
        }

        public abstract Response Handle();
    }
}
