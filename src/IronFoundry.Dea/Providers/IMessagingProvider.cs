namespace IronFoundry.Dea.Providers
{
    using System;
    using Types;

    public interface IMessagingProvider : IDisposable
    {
        Guid UniqueIdentifier { get; }

        void Publish(string subject, Message message);
        void Publish(string reply, Message message, uint delay);
        void Publish(NatsCommand argCommand, Message argMessage);
        void Publish(Message argMessage);

        void Subscribe(NatsSubscription argSubscription, Action<string, string> argCallback);

        bool Start();
        void Stop();
    }
}