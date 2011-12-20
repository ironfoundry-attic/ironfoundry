namespace IronFoundry.Dea
{
    public interface IAgent
    {
        bool Error { get; }
        bool Start();
        void Stop();
    }
}