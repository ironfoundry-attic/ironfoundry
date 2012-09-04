namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json.Linq;

    public interface IMessageHandler
    {
        bool IsLongRunning { get; }
        HandlerResponse Handle(JObject parsed);
        void OnPostReply();
    }
}