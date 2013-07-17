namespace IronFoundry.Warden.ProcessIsolation.Client
{
    using System;
    using Service;

    public interface IProcessHostClient : IProcessHostService
    {
        void Register();
        void Unregister();

        event EventHandler<ProcessEventArgs<string>> OutputReceived;
        event EventHandler<ProcessEventArgs<string>> ErrorReceived;
        event EventHandler<ProcessEventArgs<int>> ProcessExited;
        event EventHandler<EventArgs<string>> ServiceMessageReceived;
    }
}
