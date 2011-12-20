namespace IronFoundry.Dea.Config
{
    public interface IConfig
    {
        bool DisableDirCleanup { get; }
        string DropletDir { get; }
        string AppDir { get; }
        string NatsHost { get; }
        ushort NatsPort { get; }
        ushort FilesServicePort { get; }
    }
}