namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json.Linq;

    public interface IMessageHandler
    {
        HandlerResponse Handle(JObject parsed);
        void OnPostReply();
    }
}