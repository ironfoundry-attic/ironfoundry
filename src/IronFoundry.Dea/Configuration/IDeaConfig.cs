namespace IronFoundry.Dea.Configuration
{
    using System;
    using System.Net;

    public interface IDeaConfig
    {
        ushort MaxMemoryMB { get; }
        bool DisableDirCleanup { get; }
        string DropletDir { get; }
        string AppDir { get; }

        IPAddress LocalIPAddress { get; }

        ushort FilesServicePort { get; }
        Uri FilesServiceUri { get; }
        ServiceCredential FilesCredentials { get; }

        ushort MonitoringServicePort { get; }
        Uri MonitoringServiceUri { get; }
        ServiceCredential MonitoringCredentials { get; }
        string MonitoringServiceHostStr { get; }

        string AppCmdPath { get; }
        bool HasAppCmd { get; }
    }
}