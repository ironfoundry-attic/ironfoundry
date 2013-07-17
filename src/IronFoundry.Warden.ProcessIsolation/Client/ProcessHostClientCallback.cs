namespace IronFoundry.Warden.ProcessIsolation.Client
{
    using System;
    using Service;

    internal class ProcessHostClientCallback : IProcessHostClientCallback
    {
        public event EventHandler<ProcessEventArgs<string>> OnOutput;
        public event EventHandler<ProcessEventArgs<string>> OnError;
        public event EventHandler<ProcessEventArgs<int>> OnProcessExited;
        public event EventHandler<EventArgs<string>> OnServiceMessage;

        public void ProcessErrorReceived(int pid, string error)
        {
            OnError.Raise(pid, error);
        }

        public void ProcessOutputReceived(int pid, string output)
        {
            OnOutput.Raise(pid, output);
        }

        public void ProcessExit(int pid, int exitCode)
        {
            OnProcessExited.Raise(pid, exitCode);
        }

        public void ServiceMessageReceived(string message)
        {
            OnServiceMessage.Raise(message);
        }
    }
}
