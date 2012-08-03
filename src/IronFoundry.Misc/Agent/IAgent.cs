namespace IronFoundry.Misc.Agent
{
    public interface IAgent
    {
        string Name { get; }
        bool Error { get; }
        void Start();
        void Stop();
    }
}