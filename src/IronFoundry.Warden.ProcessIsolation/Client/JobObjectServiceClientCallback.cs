namespace IronFoundry.Warden.ProcessIsolation.Client
{
    using System;
    using Service;

    internal class JobObjectServiceClientCallback : IJobObjectServiceCallback
    {
        public event EventHandler<EventArgs<string>> OnOutput;
        public event EventHandler<EventArgs<string>> OnError;
        public event EventHandler<EventArgs<int>> OnProcessExited;
        public event EventHandler<EventArgs<string>> OnServiceMessage;

        public void ProcessErrorReceived(string error)
        {
            OnError.Raise(error);
        }

        public void ProcessOutputReceived(string output)
        {
            OnOutput.Raise(output);
        }

        public void ProcessExit(int exitCode)
        {
            OnProcessExited.Raise(exitCode);
        }

        public void ServiceMessageReceived(string message)
        {
            OnServiceMessage.Raise(message);
        }
    }
}
