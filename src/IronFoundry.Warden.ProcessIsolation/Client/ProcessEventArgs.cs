namespace IronFoundry.Warden.ProcessIsolation.Client
{
    using System;

    public class ProcessEventArgs<T> : EventArgs<T>
    {
        public int ProcessId { get; set; }

        public ProcessEventArgs(int processId, T value) : base(value)
        {
            ProcessId = processId;
        }
    }
}
