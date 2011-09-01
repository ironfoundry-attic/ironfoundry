using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.Dea.Providers.Interfaces
{
    public interface IMessagingProvider : IDisposable
    {
        string UniqueIdentifier { get; }
        int Sequence { get; }
        bool CurrentlyPolling { get; }

        void Publish(string subject, string message);
        void Subscribe(string subject, Action<string,string> replyCallback);
        void Connect();
        void Poll();
    }
}
