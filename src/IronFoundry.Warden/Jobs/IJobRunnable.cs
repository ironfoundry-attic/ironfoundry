namespace IronFoundry.Warden.Jobs
{
    using System.Threading.Tasks;

    public interface IJobRunnable
    {
        IJobResult Run();
        void Cancel();
        Task<IJobResult> RunAsync();
        IJobStatus Status { get; }
    }
}
