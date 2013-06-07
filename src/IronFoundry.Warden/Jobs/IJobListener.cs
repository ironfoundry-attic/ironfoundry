namespace IronFoundry.Warden.Jobs
{
    public interface IJobListener
    {
        void ObserveStatus(IJobStatus jobStatus);
    }
}
