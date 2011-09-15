namespace CloudFoundry.Net.Dea.Providers
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

        string UniqueIdentifier { get; }
        int Sequence { get; }

        void Publish(string subject, string message);
        void Publish(string argSubject, Message argMessage);
        void Subscribe(string subject, Action<string, string> replyCallback);
        // TODO void Subscribe<TMsg>(Action<TMsg> argCallback);
        bool Connect();
        void Start();
        void Stop();
    }
}