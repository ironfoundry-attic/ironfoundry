namespace IronFoundry.Bosh.Configuration
{
    using System;
    using System.IO;

    public class BoshConfig : IBoshConfig
    {
        private const string DefaultBaseDir = @"C:\IronFoundry"; // /var/vcap
        private const string DefaultBoshDirName = @"BOSH";
        private const string SettingsFileName = @"settings.json";
        private const string StateFileName = @"state.yml";

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

        public Uri Mbus { get; set; }
        public string AgentID { get; set; }
    }
}