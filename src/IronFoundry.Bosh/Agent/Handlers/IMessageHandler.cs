namespace IronFoundry.Bosh.Agent.Handlers
{
    using System;
    using Newtonsoft.Json.Linq;

    public interface IMessageHandler : IDisposable
    {
        bool IsLongRunning { get; }
        HandlerResponse Handle(JObject parsed);
        void OnPostReply();
    }
}