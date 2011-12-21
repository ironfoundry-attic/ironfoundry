namespace IronFoundry.Dea.Config
{
    using System;
    using System.Net;

    public interface IConfig
    {
        bool DisableDirCleanup { get; }
        string DropletDir { get; }
        string AppDir { get; }
        string NatsHost { get; }
        ushort NatsPort { get; }
        ushort FilesServicePort { get; }
        FilesServiceCredentials FilesCredentials { get; }
        IPAddress LocalIPAddress { get; }

        Uri FilesServiceUri { get; }
        Uri WCFFilesServiceUri { get; }
    }
}