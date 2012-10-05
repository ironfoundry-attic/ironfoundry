namespace IronFoundry.Bosh.Configuration
{
    using System;
    using IronFoundry.Bosh.Types;
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

        object VM { get; }

        string BlobstorePlugin { get; }
        Uri BlobstoreEndpoint { get; }
        string BlobstoreUser { get; }
        string BlobstorePassword { get; }

        void UpdateFrom(JObject settings);

        string BoshProtocol { get; }

        void SetState(Spec spec);
        HeartbeatStateData HeartbeatStateData { get; }

        TimeSpan HeartbeatInterval { get; }
    }
}