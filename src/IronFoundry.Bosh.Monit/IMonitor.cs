namespace IronFoundry.Bosh.Monit
{
    public interface IMonitor
    {
        void Start();
        void Stop();
        object Status();
    }
}