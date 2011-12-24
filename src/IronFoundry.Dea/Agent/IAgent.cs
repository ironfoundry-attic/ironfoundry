namespace IronFoundry.Dea.Agent
{
    public interface IAgent
    {
        bool Error { get; }
        void Start();
        void Stop();
    }
}