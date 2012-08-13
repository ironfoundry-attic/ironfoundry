namespace IronFoundry.Bosh.Agent
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using IronFoundry.Bosh.Properties;
    using IronFoundry.Misc.Agent;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Nats.Client;
    using IronFoundry.Nats.Configuration;
    using Newtonsoft.Json.Linq;

    public sealed class BoshAgent : IAgent
    {
        private readonly ushort NatsRetries = 10;
        private readonly TimeSpan NatsReconnectSleep = TimeSpan.FromSeconds(1);
        private readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(60);

        private readonly ILog log;
        private readonly INatsClient natsClient;

        private string settingsJsonStr;
        private string agentID;
        private string natsUriStr;

        public BoshAgent(ILog log, INatsClient natsClient)
        {
            this.log = log;
            this.natsClient = natsClient;
        }

        public string Name
        {
            get { return "BOSH"; }
        }

        public bool Error
        {
            get { return false; } // TODO
        }

        public void Start()
        {
            /*
             * agent/lib/agent.rb
             * Takes command line args in agent/bin/agent
             * Bootstrap!
             * Configure and enable Monit
             * Starts the Handler
             */
            BoshAgentInfrastructureVsphereSettings_LoadSettings();

            JObject settings = JObject.Parse(settingsJsonStr);
            agentID = (string)settings["agent_id"];
            natsUriStr = (string)settings["mbus"];

            // TODO string baseDir = @"C:\BOSH";

            StartHandler();
        }

        private void StartHandler()
        {
            // agent/lib/agent/handler.rb

            // find_message_processors

            var config = new BoshAgentNatsConfig(natsUriStr);
            natsClient.UseConfig(config);

            ushort natsFailCount = 0;
            while (false == natsClient.Start())
            {
                ++natsFailCount;
                if (natsFailCount > NatsRetries)
                {
                    // TODO move to BoshHandler class, custom exceptions
                    string msg = String.Format(Resources.BoshAgent_UnableToConnectAfterRetries_Fmt, natsFailCount);
                    log.Fatal(msg);
                    throw new Exception(msg);
                }
                else
                {
                    Thread.Sleep(NatsReconnectSleep);
                }
            }

            SetupSubscriptions();

            SetupHeartbeats();
            SetupSshdMonitor();
            /*
            if @process_alerts
              if (@smtp_port.nil? || @smtp_user.nil? || @smtp_password.nil?)
                @logger.error "Cannot start alert processor without having SMTP port, user and password configured"
                @logger.error "Agent will be running but alerts will NOT be properly processed"
              else
                @logger.debug("SMTP: #{@smtp_password}")
                @processor = Bosh::Agent::AlertProcessor.start("127.0.0.1", @smtp_port, @smtp_user, @smtp_password)
              end
            end
             */
        }

        private void SetupSubscriptions()
        {
            var agentSubscription = new BoshAgentSubscription(agentID);
            natsClient.Subscribe(agentSubscription, ProcessAgentMessage);
        }

        private void ProcessAgentMessage(string message, string reply)
        {
        }

        public void Stop()
        {
            natsClient.Stop();
        }

        private void BoshAgentInfrastructureVsphereSettings_LoadSettings()
        {
            // settings_file (default) /var/vcap/bosh/settings.json
            // load cdrom settings
            bool found = false;
            for (int i = 0; i < 5 && false == found; ++i)
            {
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom && d.IsReady))
                {
                    DirectoryInfo rootDirectory = drive.RootDirectory;
                    string envPath = Path.Combine(rootDirectory.FullName, "env");
                    if (File.Exists(envPath))
                    {
                        settingsJsonStr = File.ReadAllText(envPath);
                        Settings.Default.SettingsJson = settingsJsonStr;
                        Settings.Default.Save();
                        EjectMedia.Eject(rootDirectory.FullName);
                        found = true;
                        break;
                    }
                }
                if (false == found)
                {
                    log.Warn("No CD rom drives ready. Waiting...");
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }

            if (settingsJsonStr.IsNullOrEmpty())
            {
                throw new Exception(); // Should be LoadSettingsException
            }
        }

        private void SetupHeartbeats()
        {
            throw new NotImplementedException();
        }

        private void SetupSshdMonitor()
        {
            // TODO: how could we do this on Windows?
        }

        private class BoshAgentNatsConfig : INatsConfig
        {
            private readonly string host;
            private readonly ushort port;
            private readonly string user;
            private readonly string password;

            public BoshAgentNatsConfig(string natsUriStr)
            {
                // nats://nats:nats@172.21.10.181:4222
                var natsUri = new Uri(natsUriStr);
                this.host = natsUri.Host;
                this.port = (ushort)natsUri.Port;

                string[] userInfo = natsUri.UserInfo.Split(new[] { ':' });
                this.user = userInfo[0];
                this.password = userInfo[1];
            }

            public string Host { get { return host; } }

            public ushort Port { get { return port; } }

            public string User { get { return user; } }

            public string Password { get { return password; } }
        }
    }
}