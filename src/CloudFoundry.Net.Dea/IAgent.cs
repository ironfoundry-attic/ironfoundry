namespace CloudFoundry.Net.Dea
{
    public interface IAgent
    {
        void Start();
        void Stop();
        void ProcessDropletStatus(string message, string reply);
        void ProcessDeaFindDroplet(string message, string reply);
        void ProcessDeaStatus(string message, string reply);
        void ProcessDeaDiscover(string message, string reply);
        void ProcessDeaStop(string message, string reply);
        void ProcessDeaStart(string message, string reply);
        void ProcessDeaUpdate(string message, string reply);
        void ProcessRouterStart(string message, string reply);
        void ProcessHealthManagerStart(string message, string reply);
    }
}