namespace IronFoundry.Bosh.Agent.Handlers
{
    using System;
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public abstract class BaseMessageHandler : IMessageHandler
    {
        private bool disposed = false;
        protected readonly IBoshConfig config;

        public BaseMessageHandler(IBoshConfig config)
        {
            this.config = config;
        }

        public abstract HandlerResponse Handle(JObject parsed);

        public virtual bool IsLongRunning
        {
            get { return false; }
        }

        public virtual void OnPostReply() { }

        public void Dispose()
        {
            if (false == disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing) { }
    }
}