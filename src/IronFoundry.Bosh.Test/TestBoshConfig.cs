namespace IronFoundry.Bosh.Test
{
    using System;
    using System.IO;
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class TestBoshConfig : IBoshConfig
    {
        private readonly string baseDir = Path.GetTempPath();
        private readonly string boshBaseDir;
        private readonly string settingsFilePath;
        private readonly string stateFilePath;
        private readonly Uri mbus = new Uri("nats://localhost:4222");
        private readonly string agentID = Guid.NewGuid().ToString().ToLowerInvariant();
        private readonly Uri blobstoreEndpoint = new Uri("http://172.21.10.181:25250");

        public TestBoshConfig()
        {
            boshBaseDir = Path.Combine(baseDir, BoshConfig.DefaultBoshDirName);
            settingsFilePath = Path.Combine(boshBaseDir, BoshConfig.SettingsFileName);
            stateFilePath = Path.Combine(boshBaseDir, BoshConfig.StateFileName);
        }

        public bool Debugging { get { return false; } }
        public string BaseDir { get { return baseDir; } }
        public string BoshBaseDir { get { return boshBaseDir; } }
        public string SettingsFilePath { get { return settingsFilePath; } }
        public string StateFilePath { get { return stateFilePath; } }
        public Uri Mbus { get { return mbus; } }
        public string AgentID { get { return agentID; } }
        public string BlobstorePlugin { get { return "simple"; } }
        public Uri BlobstoreEndpoint { get { return blobstoreEndpoint; } }
        public string BlobstoreUser { get { return "agent"; } }
        public string BlobstorePassword { get { return "agent"; } }
        public void UpdateFrom(JObject settings) { }
    }
}