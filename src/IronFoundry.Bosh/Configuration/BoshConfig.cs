namespace IronFoundry.Bosh.Configuration
{
    using System;
#if DEBUG
    using System.Configuration;
#endif
    using System.IO;
    using Newtonsoft.Json.Linq;

    public class BoshConfig : IBoshConfig
    {
        private const string BOSH_PROTOCOL = "1"; // agent/lib/agent/version.rb
        public const string DefaultBaseDir = @"C:\IronFoundry"; // /var/vcap
        public const string DefaultBoshDirName = @"BOSH";
        public const string SettingsFileName = @"settings.json";
        public const string StateFileName = @"state.yml";

#if DEBUG
        public const string BoshAgentDebugging_AppSettingKey = @"BoshAgentDebugging";
        private readonly bool debugging = false;
#endif

        private readonly string baseDir;
        private readonly string boshBaseDir;
        private readonly string settingsFilePath;
        private readonly string stateFilePath;

        public BoshConfig()
        {
#if DEBUG
            if (false == Boolean.TryParse(ConfigurationManager.AppSettings[BoshAgentDebugging_AppSettingKey], out debugging))
            {
                debugging = false;
            }
#endif
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

#if DEBUG
        public bool Debugging { get { return debugging; } }
#endif

        public string BaseDir { get { return baseDir; } }
        public string BoshBaseDir { get { return boshBaseDir; } }
        public string SettingsFilePath { get { return settingsFilePath; } }
        public string StateFilePath { get { return stateFilePath; } }

        public Uri Mbus { get; private set; }
        public string AgentID { get; private set; }
        public object VM { get; private set; } // TODO real object

        public string BlobstorePlugin { get; private set; }
        public Uri BlobstoreEndpoint { get; private set; }
        public string BlobstoreUser { get; private set; }
        public string BlobstorePassword { get; private set; }

        public string BoshProtocol { get { return BOSH_PROTOCOL; } }

        public void UpdateFrom(JObject settings)
        {
            AgentID = (string)settings["agent_id"];
            Mbus = new Uri((string)settings["mbus"]);
            VM = settings["vm"];

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