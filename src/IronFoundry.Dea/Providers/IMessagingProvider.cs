namespace IronFoundry.Dea.Providers
{
    using System;
    using Types;

    public enum NatsMessagingStatus
    {
        RUNNING,
        STOPPING,
        STOPPED,
        ERROR,
    }

    public interface IMessagingProvider : IDisposable
    {
        NatsMessagingStatus Status { get; }
        string StatusMessage { get; }

        Guid UniqueIdentifier { get; }
        int Sequence { get; }

        void Publish(string subject, Message message);
        void Publish(NatsCommand argCommand, Message argMessage);
        void Publish(Message argMessage);

        void Subscribe(NatsSubscription argSubscription, Action<string, string> argCallback);

        bool Connect();
        void Start();
        void Stop();
    }
}