namespace IronFoundry.Warden.Protocol
{
    public interface ITaskRequest
    {
        string Handle { get; }
        bool Privileged { get; }
        ResourceLimits Rlimits { get; }
        string Script { get; }
    }
}
