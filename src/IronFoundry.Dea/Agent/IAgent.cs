namespace IronFoundry.Dea.Agent
{
    public interface IAgent
    {
        bool Error { get; }
        bool Start();
        void Stop();
    }
}