namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json.Linq;

    public abstract class BaseMessageHandler : IMessageHandler
    {
        public abstract HandlerResponse Handle(JObject parsed);

        public virtual void OnPostPublish() { }
    }
}