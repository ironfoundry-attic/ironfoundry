namespace IronFoundry.Nats.Client
{
    using System;
using IronFoundry.Nats.Configuration;

    public interface INatsClient : IDisposable
    {
        Guid UniqueIdentifier { get; }

        void Publish(string subject, INatsMessage message);
        void Publish(string reply, INatsMessage message, uint delay);
        void Publish(NatsCommand argCommand, INatsMessage argMessage);
        void Publish(INatsMessage argMessage);

        void Subscribe(INatsSubscription argSubscription, Action<string, string> argCallback);

        void UseConfig(INatsConfig natsConfig);
        bool Start();
        void Stop();
    }
}