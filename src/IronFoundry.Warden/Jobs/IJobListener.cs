namespace IronFoundry.Warden.Jobs
{
    public interface IJobListener
    {
        void ListenStatus(IJobStatus jobStatus);
    }
}
