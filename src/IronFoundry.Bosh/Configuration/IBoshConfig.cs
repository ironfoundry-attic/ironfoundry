namespace IronFoundry.Bosh.Configuration
{
    using System;
    using Newtonsoft.Json.Linq;

    public interface IBoshConfig
    {
#if DEBUG
        bool Debugging { get; }
#endif

        string BaseDir { get; }
        string BoshBaseDir { get; }
        string SettingsFilePath { get; }
        string StateFilePath { get; }

        Uri Mbus { get; }
        string AgentID { get; }

        string BlobstorePlugin { get; }
        Uri BlobstoreEndpoint { get; }
        string BlobstoreUser { get; }
        string BlobstorePassword { get; }

        void UpdateFrom(JObject settings);
    }
}