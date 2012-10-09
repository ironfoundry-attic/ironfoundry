namespace IronFoundry.Bosh.Configuration
{
    using System;
#if DEBUG
    using System.Configuration;
#endif
    using System.IO;
    using Newtonsoft.Json.Linq;
using IronFoundry.Bosh.Types;
    using Newtonsoft.Json;

    public class BoshConfig : IBoshConfig
    {
        private const string BOSH_PROTOCOL = "1"; // agent/lib/agent/version.rb

        public const string DefaultBaseDir = @"C:\IronFoundry"; // /var/vcap
        public const string DefaultBoshDirName = @"BOSH";
        public const string SettingsFileName = @"settings.json";
        public const string StateFileName = @"state.json";

#if DEBUG
        public const string BoshAgentDebugging_AppSettingKey = @"BoshAgentDebugging";
        private readonly bool debugging = false;
#endif

        private readonly string baseDir;
        private readonly string boshBaseDir;
        private readonly string settingsFilePath;
        private readonly string stateFilePath;

        private static readonly TimeSpan heartbeatInterval = TimeSpan.FromSeconds(60);

        private Spec spec;

        public BoshConfig()
        {
#if DEBUG
            if (false == Boolean.TryParse(ConfigurationManager.AppSettings[BoshAgentDebugging_AppSettingKey], out debugging))
            {
                debugging = false;
            }
#endif
            baseDir = DefaultBaseDir;
            boshBaseDir = Path.Combine(baseDir, DefaultBoshDirName);
            settingsFilePath = Path.Combine(boshBaseDir, SettingsFileName);
            stateFilePath = Path.Combine(boshBaseDir, StateFileName);

            Mbus = new Uri("nats://localhost:4222");
            AgentID = "not_configured";

            Directory.CreateDirectory(BoshBaseDir);

            ReadState(); // TODO state file should be wrapped in class managing it.
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

        public TimeSpan HeartbeatInterval { get { return heartbeatInterval; } }

        public HeartbeatStateData HeartbeatStateData
        {
            get
            {
                HeartbeatStateData rv;

                if (null == this.spec)
                {
                    rv = new HeartbeatStateData("unknown", 0, "unknown"); // TODO Job State
                }
                else
                {
                    rv = new HeartbeatStateData(spec.Job.Name, spec.Index, "running"); // TODO Job State
                }

                return rv;
            }
        }

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

        public void SetState(Spec spec)
        {
            this.spec = spec;
            using (var fs = File.Open(stateFilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var tw = new StreamWriter(fs))
                {
                    using (var jw = new JsonTextWriter(tw))
                    {
                        jw.Indentation = 2;
                        jw.IndentChar = ' ';
                        jw.Formatting = Formatting.Indented;
                        var serializer = new JsonSerializer();
                        serializer.Serialize(jw, this.spec);
                    }
                }
            }
        }

        private void ReadState()
        {
            if (File.Exists(stateFilePath))
            {
                using (var fs = File.Open(stateFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var tr = new StreamReader(fs))
                    {
                        using (var jr = new JsonTextReader(tr))
                        {
                            var serializer = new JsonSerializer();
                            this.spec = serializer.Deserialize<Spec>(jr);
                        }
                    }
                }
            }
            else
            {
                this.spec = new Spec();
            }
        }
    }
}