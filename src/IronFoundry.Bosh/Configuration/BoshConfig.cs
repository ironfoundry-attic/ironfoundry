namespace IronFoundry.Bosh.Configuration
{
    using System;
    using System.IO;
    using Newtonsoft.Json.Linq;

    public class BoshConfig : IBoshConfig
    {
        public const string DefaultBaseDir = @"C:\IronFoundry"; // /var/vcap
        public const string DefaultBoshDirName = @"BOSH";
        public const string SettingsFileName = @"settings.json";
        public const string StateFileName = @"state.yml";

        private readonly string baseDir;
        private readonly string boshBaseDir;
        private readonly string settingsFilePath;
        private readonly string stateFilePath;

        public BoshConfig()
        {
            /*
             * unconfigured defaults from agent/lib/agent.rb
              options = {
                "configure"         => true,
                "logging"           => { "level" => "DEBUG" },
                "mbus"              => "nats://localhost:4222",
                "agent_id"          => "not_configured",
                "base_dir"          => "/var/vcap",
                "platform_name"     => "ubuntu",
                "blobstore_options" => {}
              }
             */
            baseDir = DefaultBaseDir;
            boshBaseDir = Path.Combine(baseDir, DefaultBoshDirName);
            settingsFilePath = Path.Combine(boshBaseDir, SettingsFileName);
            stateFilePath = Path.Combine(boshBaseDir, StateFileName);

            Mbus = new Uri("nats://localhost:4222");
            AgentID = "not_configured";

            Directory.CreateDirectory(BoshBaseDir);
        }

        public string BaseDir { get { return baseDir; } }
        public string BoshBaseDir { get { return boshBaseDir; } }
        public string SettingsFilePath { get { return settingsFilePath; } }
        public string StateFilePath { get { return stateFilePath; } }

        public Uri Mbus { get; private set; }
        public string AgentID { get; private set; }

        public string BlobstorePlugin { get; private set; }
        public Uri BlobstoreEndpoint { get; private set; }
        public string BlobstoreUser { get; private set; }
        public string BlobstorePassword { get; private set; }

        public void UpdateFrom(JObject settings)
        {
            AgentID = (string)settings["agent_id"];
            Mbus = new Uri((string)settings["mbus"]);

            var bs = settings["blobstore"];
            if (bs != null)
            {
                BlobstorePlugin = (string)bs["plugin"];
                var bsp = bs["properties"];
                BlobstoreEndpoint = new Uri((string)bsp["endpoint"]);
                BlobstoreUser = (string)bsp["user"];
                BlobstorePassword = (string)bsp["password"];
            }
        }
    }
}