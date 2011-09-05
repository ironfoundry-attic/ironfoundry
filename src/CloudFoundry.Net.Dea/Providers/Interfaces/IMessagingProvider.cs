namespace CloudFoundry.Net.Dea.Providers.Interfaces
{
    using System;

    public interface IMessagingProvider : IDisposable
    {
        string UniqueIdentifier { get; }
        int Sequence { get; }

        void Publish(string subject, string message);
        void Subscribe(string subject, Action<string,string> replyCallback);
        void Connect();
        void Poll();
    }
}