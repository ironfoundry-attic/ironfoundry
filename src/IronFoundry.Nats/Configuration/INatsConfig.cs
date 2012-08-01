namespace IronFoundry.Nats.Configuration
{
    public interface INatsConfig
    {
        string Host { get; }
        ushort Port { get; }
        string User { get; }
        string Password { get; }
    }
}