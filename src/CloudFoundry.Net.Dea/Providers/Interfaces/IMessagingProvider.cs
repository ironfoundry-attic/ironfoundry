namespace CloudFoundry.Net.Dea.Providers.Interfaces
{
    using System;
    using Types.Messages;

    public interface IMessagingProvider : IDisposable
    {
        string UniqueIdentifier { get; }
        int Sequence { get; }

        void Publish(string subject, string message);
        void Publish(string argSubject, Message argMessage);
        void Subscribe(string subject, Action<string, string> replyCallback);
        // TODO void Subscribe<TMsg>(Action<TMsg> argCallback);
        void Connect();
        void Poll();
    }
}