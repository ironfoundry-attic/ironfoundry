namespace IronFoundry.Warden.Containers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NLog;
    using ProcessIsolation;
    using ProcessIsolation.Client;

    public class ContainerProcessIORouter : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly List<ContainerProcessIOHandler> ioHandlers = new List<ContainerProcessIOHandler>();
        private IProcessHostClient jobObjectClient;

        public ContainerProcessIORouter(IProcessHostClient jobObjectClient)
        {
            this.jobObjectClient = jobObjectClient;

            this.jobObjectClient.ServiceMessageReceived += ClientOnServiceMessageReceived;
            this.jobObjectClient.OutputReceived += ClientOnOutputReceived;
            this.jobObjectClient.ErrorReceived += ClientOnErrorReceived;
            this.jobObjectClient.ProcessExited += ClientOnProcessExited;
        }

        private void ClientOnServiceMessageReceived(object sender, EventArgs<string> processEventArgs)
        {
            log.Info("ProcessHost Message: {0}", processEventArgs.Value);
        }

        private void ClientOnProcessExited(object sender, ProcessEventArgs<int> processEventArgs)
        {
            ioHandlers.Where(h => h.ProcessID == processEventArgs.ProcessId).All(h => { h.OnExit(processEventArgs.Value); return true; });
        }

        private void ClientOnOutputReceived(object sender, ProcessEventArgs<string> processEventArgs)
        {
            ioHandlers.Where(h => h.ProcessID == processEventArgs.ProcessId).All(h => { h.OnOutput(processEventArgs.Value); return true; });
        }

        private void ClientOnErrorReceived(object sender, ProcessEventArgs<string> processEventArgs)
        {
            ioHandlers.Where(h => h.ProcessID == processEventArgs.ProcessId).All(h => { h.OnError(processEventArgs.Value); return true; });
        }

        public void AddProcessIO(int pid, Action<string> onOutput, Action<string> onError, Action<int> onExit)
        {
            ioHandlers.Add(new ContainerProcessIOHandler
            {
                ProcessID = pid,
                OnAdminMessage = onOutput,
                OnOutput = onOutput,
                OnError = onError,
                OnExit = onExit
            });
        }

        public void RemoveProcessIO(int pid)
        {
            ioHandlers.RemoveAll(x => x.ProcessID == pid);
        }

        public void Dispose()
        {
            if (jobObjectClient != null)
            {
                jobObjectClient.ServiceMessageReceived += ClientOnServiceMessageReceived;
                jobObjectClient.OutputReceived += ClientOnOutputReceived;
                jobObjectClient.ErrorReceived += ClientOnErrorReceived;
                jobObjectClient.ProcessExited += ClientOnProcessExited;
                jobObjectClient = null;
            }
        }

        private class ContainerProcessIOHandler
        {
            public int ProcessID { get; set; }
            public Action<string> OnOutput { get; set; }
            public Action<string> OnError { get; set; }
            public Action<string> OnAdminMessage { get; set; }
            public Action<int> OnExit { get; set; }
        }
    }
}
