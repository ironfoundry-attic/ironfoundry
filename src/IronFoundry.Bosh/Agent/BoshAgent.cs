namespace IronFoundry.Bosh.Agent
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using IronFoundry.Bosh.Agent.Handlers;
    using IronFoundry.Bosh.Properties;
    using IronFoundry.Misc.Agent;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.Utilities;
    using IronFoundry.Nats.Client;
    using IronFoundry.Nats.Configuration;
    using Newtonsoft.Json.Linq;
    using StructureMap;

    public sealed class BoshAgent : IAgent
    {
        private const string DefaultBaseDir = @"C:\IronFoundry"; // /var/vcap
        private const string BoshDirName = "BOSH";
        private const string SettingsFileName = @"settings.json";
        private const string StateFileName = @"state.yml";

        private readonly string SettingsFilePath = Path.Combine(DefaultBaseDir, BoshDirName, SettingsFileName);
        private readonly string StateFilePath = Path.Combine(DefaultBaseDir, BoshDirName, StateFileName);

        private readonly ushort NatsRetries = 10;
        private readonly TimeSpan NatsReconnectSleep = TimeSpan.FromSeconds(1);
        private readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(60);

        private readonly IContainer ioc;
        private readonly ILog log;
        private readonly INatsClient natsClient;

        private JObject settings;
        private string agentID;
        private string natsUriStr;

        private HeartbeatProcessor heartbeatProcessor;

        public BoshAgent(IContainer ioc, ILog log, INatsClient natsClient)
        {
            this.ioc = ioc;
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
            InitializeDirectories();

            BoshAgentInfrastructureVsphereSettings_LoadSettings();

            agentID = (string)settings["agent_id"];
            natsUriStr = (string)settings["mbus"];

            /*
             * TODO:
             * run sysprep
                 set admin password (via unattend.xml)
             * set ip address
             * set licensing server
netsh interface ipv4 set address name="Local Area Connection" source=static address=%1 mask=%2 gateway=%3
netsh interface ipv4 set dns name="Local Area Connection" source=static addr=%4
netsh interface ipv4 add dns name="Local Area Connection" addr=%5
             */
            Sysprep();
            SetupNetworking();

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

            // setup heartbeats
            heartbeatProcessor = new HeartbeatProcessor(log, natsClient, agentID, TimeSpan.FromSeconds(1));
            heartbeatProcessor.Start();

            // SetupSshdMonitor();
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

        private void SetupNetworking()
        {
            /*
             * TODO:
             * set ip address
netsh interface ipv4 set address name="Local Area Connection" source=static address=%1 mask=%2 gateway=%3
netsh interface ipv4 set dns name="Local Area Connection" source=static addr=%4
netsh interface ipv4 add dns name="Local Area Connection" addr=%5
             */
            var network_setup_setting = settings["vm"]["network_setup"];
            if (null != network_setup_setting && (bool)network_setup_setting)
            {
                return;
            }
            var network = settings["networks"].First;
            if (network.HasValues)
            {
            	var net = network.First;
                string ip = (string)net["ip"];
            	string netmask = (string)net["netmask"];
                string gateway = (string)net["gateway"];

                string args = String.Format(
                    @"interface ipv4 set address name=""Local Area Connection"" source=static address={0} mask={1} gateway={2}",
                    ip, netmask, gateway);

                bool err = false;
                var exec = new ExecCmd(log, "netsh", args);
                ExecCmdResult rslt = exec.Run();
                if (rslt.Success)
                {
                    bool firstDns = true;
                    foreach (string dnsStr in net["dns"])
                    {
                        if (firstDns)
                        {
                            args = String.Format(@"netsh interface ipv4 set dns name=""Local Area Connection"" source=static addr={0}", dnsStr);
                            firstDns = false;
                        }
                        else
                        {
                            args = String.Format(@"netsh interface ipv4 add dns name=""Local Area Connection"" addr={0}", dnsStr);
                        }
                        exec = new ExecCmd(log, "netsh", args);
                        rslt = exec.Run();
                        if (false == rslt.Success)
                        {
                            // TODO
                            err = true;
                        }
                    }
                }
                else
                {
                    // TODO
                    err = true;
                }
                if (false == err)
                {
                    settings["vm"]["network_setup"] = true;
                    SaveSettings();
                }
            }
        }

        private void Sysprep()
        {
            var sysprepped_setting = settings["vm"]["sysprepped"];
            if (null != sysprepped_setting && (bool)sysprepped_setting)
            {
                return;
            }

            string unattendXml = Resources.UnattendXML;
            var xdoc = XDocument.Parse(unattendXml);
            XNamespace ns = xdoc.Root.GetDefaultNamespace();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(new NameTable());
            nsMgr.AddNamespace("ns", ns.NamespaceName);

            // ComputerName
            var eleComputerName = xdoc.XPathSelectElement(@"/ns:unattend/ns:settings/ns:component/ns:ComputerName", nsMgr);
            string computerName = (string)settings["vm"]["name"];
            eleComputerName.Value = computerName;

            // RegisteredOrganization
            var elements = xdoc.XPathSelectElements(@"//ns:component/ns:RegisteredOrganization", nsMgr);
            foreach (var ele in elements)
            {
                ele.Value = "ORG TODO"; // TODO
            }

            // RegisteredOwner
            elements = xdoc.XPathSelectElements(@"//ns:component/ns:RegisteredOwner", nsMgr);
            foreach (var ele in elements)
            {
                ele.Value = "OWNER TODO"; // TODO
            }

            string pathToUnattend = Path.GetTempFileName();
            using (var writer = new XmlTextWriter(pathToUnattend, null))
            {
                xdoc.WriteTo(writer);
            }
            var cmd = new ExecCmd(log, @"C:\sysadmin\sysprep\sysprep.exe", "/generalize /oobe /reboot /unattend:" + pathToUnattend); // TODO: /reboot ?
            log.Info("Executing: '{0}'", cmd);
            ExecCmdResult rslt = cmd.Run();
            log.Info("Result: '{0}'", rslt);

            if (rslt.Success)
            {
                settings["vm"]["sysprepped"] = true;
                SaveSettings();
            }
        }

        private void InitializeDirectories()
        {
            Directory.CreateDirectory(DefaultBaseDir);
            Directory.CreateDirectory(Path.Combine(DefaultBaseDir, BoshDirName));
        }

        private void SetupSubscriptions()
        {
            var agentSubscription = new BoshAgentSubscription(agentID);
            natsClient.Subscribe(agentSubscription, HandleAgentMessage);
        }

        private void ReplyToAgentMessage(string replyTo, object payload)
        {
            log.Info("reply_to: {0}, payload: {1}", replyTo, payload);

            // encryption code if @credentials

            /*
    # TODO once we upgrade to nats 0.4.22 we can use
    # NATS.server_info[:max_payload] instead of NATS_MAX_PAYLOAD_SIZE
    NATS_MAX_PAYLOAD_SIZE = 1024 * 1024
      json = Yajl::Encoder.encode(payload)

      # TODO figure out if we want to try to scale down the message instead
      # of generating an exception
      if json.bytesize < NATS_MAX_PAYLOAD_SIZE
        EM.next_tick do
          @nats.publish(reply_to, json, blk)
        end
      else
        msg = "message > NATS_MAX_PAYLOAD, stored in blobstore"
        original = @credentials ? payload : unencrypted
        exception = RemoteException.new(msg, nil, original)
        @logger.fatal(msg)
        EM.next_tick do
          @nats.publish(reply_to, exception.to_hash, blk)
        end
      end
             */
        }

        private void HandleAgentMessage(string message, string reply)
        {
            JObject j = JObject.Parse(message);

            string replyTo = (string)j["reply_to"];
            if (replyTo.IsNullOrWhiteSpace())
            {
                log.Error(Resources.BoshAgent_MissingReplyTo_Fmt, message);
                return;
            }

            log.Debug(Resources.BoshAgent_AgentMessage_Fmt, message);

            // encryption code here

            string method = (string)j["method"];
            if (method == "get_state")
            {
                method = "state";
            }

            IMessageHandler handler;
            try
            {
                handler = ioc.GetInstance<IMessageHandler>(method);
            }
            catch (StructureMapException ex)
            {
                log.Error(ex, Resources.BoshAgent_MissingHandlerForMethod_Fmt, method);
                return;
            }

            /*
             * NB: long running tasks get a task ID that is sent to the director,
      if processor
        Thread.new { process_in_thread(processor, reply_to, method, args) }
      elsif method == "get_task"
        handle_get_task(reply_to, args.first)
      elsif method == "shutdown"
        handle_shutdown(reply_to)
      else
        re = RemoteException.new("unknown message #{msg.inspect}")
        publish(reply_to, re.to_hash)
    def process_long_running(reply_to, processor, args)
      agent_task_id = generate_agent_task_id

      @long_running_agent_task = [agent_task_id]

      payload = {:value => {:state => "running", :agent_task_id => agent_task_id}}
      publish(reply_to, payload)

      result = process(processor, args)
      @results << [Time.now.to_i, agent_task_id, result]
      @long_running_agent_task = []
    end
             */

            HandlerResponse response;
            try
            {
                response = handler.Handle(j);
            }
            catch (Exception ex)
            {
                log.Error(ex, Resources.BoshAgent_ExceptionHandlingMethod_Fmt, method);
                return;
            }

            Publish(replyTo, response);
            handler.OnPostReply();
        }

        private void Publish(string replyTo, HandlerResponse response)
        {
            const uint NATS_MAX_PAYLOAD_SIZE = 1024 * 1024;

            // TODO encrypt?

            string responseJsonStr = response.ToJson();
            int responseJsonSize = Encoding.ASCII.GetByteCount(responseJsonStr);

            log.Debug(Resources.BoshAgent_ResponseDebug_Fmt, replyTo, responseJsonStr);

            /*
              TODO figure out if we want to try to scale down the message instead of generating an exception
             */
            if (responseJsonSize < NATS_MAX_PAYLOAD_SIZE)
            {
                natsClient.PublishReply(replyTo, response, 0);
            }
            else
            {
                // TODO OOPS
                log.Error(Resources.BoshAgent_ResponseJsonTooLarge_Fmt, responseJsonSize, NATS_MAX_PAYLOAD_SIZE);
            }
            /*
      if json.bytesize < NATS_MAX_PAYLOAD_SIZE
        EM.next_tick do
          @nats.publish(reply_to, json, blk)
        end
      else
        msg = "message > NATS_MAX_PAYLOAD, stored in blobstore"
        original = @credentials ? payload : unencrypted
        exception = RemoteException.new(msg, nil, original)
        @logger.fatal(msg)
        EM.next_tick do
          @nats.publish(reply_to, exception.to_hash, blk)
        end
      end
    end
             */
        }

        public void Stop()
        {
            if (null != heartbeatProcessor)
            {
                heartbeatProcessor.Stop();
            }
            if (null != natsClient)
            {
                natsClient.Stop();
            }
        }

        private void BoshAgentInfrastructureVsphereSettings_LoadSettings()
        {
            bool settingsFound = false;
            DirectoryInfo driveRootDirectory = null;

            try
            {
                for (int i = 0; i < 5 && false == settingsFound; ++i)
                {
                    foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom && d.IsReady))
                    {
                        driveRootDirectory = drive.RootDirectory;
                        string envPath = Path.Combine(driveRootDirectory.FullName, "env");
                        if (File.Exists(envPath))
                        {
                            string settingsJsonStr = File.ReadAllText(envPath);
                            LoadSettings(settingsJsonStr);
                            SaveSettings();
                            settingsFound = true;
                            break;
                        }
                    }
                    if (false == settingsFound)
                    {
                        log.Warn("No CD rom drives ready...");
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            if (settingsFound)
            {
                EjectMedia.Eject(driveRootDirectory.FullName);
            }
            else
            {
                settingsFound = LoadSettings();
            }

            if (false == settingsFound)
            {
                throw new Exception(); // Should be LoadSettingsException
            }
        }

        private bool LoadSettings()
        {
            bool rv = false;

            if (File.Exists(SettingsFilePath))
            {
                rv = LoadSettings(File.ReadAllText(SettingsFilePath));
            }

            return rv;
        }

        private bool LoadSettings(string settingsJsonStr)
        {
            bool rv = false;
            settings = JObject.Parse(settingsJsonStr);
            return rv;
        }

        private void SaveSettings()
        {
            string settingsJsonStr = settings.ToString();
            File.WriteAllText(SettingsFilePath, settingsJsonStr);
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