namespace IronFoundry.Nats.Client
{
    using System;
    using IronFoundry.Nats.Configuration;

    public interface INatsClient : IDisposable
    {
        Guid UniqueIdentifier { get; }

        void Publish(string message);
        void Publish(string subject, INatsMessage message);
        void Publish(NatsCommand argCommand, INatsMessage argMessage);
        void Publish(INatsMessage argMessage);

        void PublishReply(string replyTo, string json, uint delay);
        void PublishReply(string replyTo, INatsMessage message, uint delay);

        void Subscribe(INatsSubscription argSubscription, Action<string, string> argCallback);

        void UseConfig(INatsConfig natsConfig);
        bool Start();
        void Stop();
    }
}