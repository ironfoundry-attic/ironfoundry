namespace IronFoundry.Warden.Jobs
{
    using System;
    using System.Threading.Tasks;

    public interface IJobRunnable
    {
        IJobResult Run();
        Task RunAsync();
        void Cancel();
        IJobStatus Status { get; }
        event EventHandler<JobStatusEventArgs> JobStatusAvailable;
    }
}
