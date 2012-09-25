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
    using IronFoundry.Bosh.Blobstore;
    using IronFoundry.Bosh.Configuration;
    using IronFoundry.Bosh.Properties;
    using IronFoundry.Misc.Agent;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.Utilities;
    using IronFoundry.Nats.Client;
    using IronFoundry.Nats.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using StructureMap;

    public sealed class BoshAgent : IAgent
    {
        private readonly ushort NatsRetries = 10;
        private readonly TimeSpan NatsReconnectSleep = TimeSpan.FromSeconds(1);
        private readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(60);

        private readonly IContainer ioc;
        private readonly ILog log;
        private readonly INatsClient natsClient;
        private readonly IBoshConfig config;
        private readonly IBlobstoreClientFactory blobstoreClientFactory;

        private JObject settings;

        private HeartbeatProcessor heartbeatProcessor;

        public BoshAgent(IContainer ioc, ILog log, INatsClient natsClient,
            IBoshConfig config, IBlobstoreClientFactory blobstoreClientFactory)
        {
            this.ioc = ioc;
            this.log = log;
            this.natsClient = natsClient;
            this.config = config;
            this.blobstoreClientFactory = blobstoreClientFactory;
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
            config.UpdateFrom(settings);

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
            bool wasSysprepped = Sysprep();
            if (wasSysprepped)
            {
                Stop();
                Environment.Exit(0); // TODO not the prettiest way to do this.
            }
            SetupNetworking();

            // agent/lib/agent/handler.rb

            // find_message_processors

            var natsConfig = new BoshAgentNatsConfig(config.Mbus);
            natsClient.UseConfig(natsConfig);

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
            heartbeatProcessor = new HeartbeatProcessor(log, natsClient, config.AgentID, TimeSpan.FromSeconds(1));
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
            VMSetupState vmSetupState = settings["vm-setup-state"].ToObject<VMSetupState>();
            if (vmSetupState.IsNetworkSetup)
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

                // TODO: Depending on "Local Area Connection" is brittle.
                string args = String.Format(
                    @"interface ipv4 set address name=""Local Area Connection"" source=static address={0} mask={1} gateway={2}",
                    ip, netmask, gateway);

                bool err = false;
                var exec = new ExecCmd(log, "netsh", args);
                ExecCmdResult rslt = exec.Run(6, TimeSpan.FromSeconds(10));
                if (rslt.Success)
                {
                    bool firstDns = true;
                    foreach (string dnsStr in net["dns"])
                    {
                        if (firstDns)
                        {
                            args = String.Format(@"interface ipv4 set dns name=""Local Area Connection"" source=static addr={0}", dnsStr);
                            firstDns = false;
                        }
                        else
                        {
                            args = String.Format(@"interface ipv4 add dns name=""Local Area Connection"" addr={0}", dnsStr);
                        }
                        exec = new ExecCmd(log, "netsh", args);
                        rslt = exec.Run(6, TimeSpan.FromSeconds(10));
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
                    vmSetupState.IsNetworkSetup = true;
                    settings["vm-setup-state"] = JObject.FromObject(vmSetupState);
                    SaveSettings();
                }
            }
        }

        private bool Sysprep()
        {
            bool wasSysprepped = false;

            VMSetupState vmSetupState = settings["vm-setup-state"].ToObject<VMSetupState>();
            if (vmSetupState.IsSysprepped)
            {
                return wasSysprepped;
            }

            string unattendXml = Resources.UnattendXML;
            var xdoc = XDocument.Parse(unattendXml);
            XNamespace ns = xdoc.Root.GetDefaultNamespace();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(new NameTable());
            nsMgr.AddNamespace("ns", ns.NamespaceName);

            // ComputerName
            var eleComputerName = xdoc.XPathSelectElement(@"/ns:unattend/ns:settings/ns:component/ns:ComputerName", nsMgr);
            string computerName = (string)settings["vm"]["name"];
            eleComputerName.Value = computerName.Substring(0, Math.Min(15, computerName.Length));

            // RegisteredOrganization
            var elements = xdoc.XPathSelectElements(@"//ns:component/ns:RegisteredOrganization", nsMgr);
            foreach (var ele in elements)
            {
                ele.Value = "Tier3"; // TODO
            }

            // RegisteredOwner
            elements = xdoc.XPathSelectElements(@"//ns:component/ns:RegisteredOwner", nsMgr);
            foreach (var ele in elements)
            {
                ele.Value = "Tier3"; // TODO
            }

            string pathToUnattend = Path.GetTempFileName();
            using (var writer = XmlWriter.Create(pathToUnattend))
            {
                xdoc.WriteTo(writer);
            }
            var cmd = new ExecCmd(log, @"C:\sysadmin\sysprep\sysprep.exe", "/quit /generalize /oobe /unattend:" + pathToUnattend);
            log.Info("Executing: '{0}'", cmd);
            ExecCmdResult rslt = cmd.Run();
            log.Info("Result: '{0}'", rslt);
            if (rslt.Success)
            {
                wasSysprepped = vmSetupState.IsSysprepped = true;
                settings["vm-setup-state"] = JObject.FromObject(vmSetupState);
                SaveSettings();
            }
            cmd = new ExecCmd(log, @"C:\Windows\System32\shutdown.exe", "/r /t 10 /c BOSHAgent /d p:4:2");
            log.Info("Executing: '{0}'", cmd);
            rslt = cmd.Run();
            log.Info("Result: '{0}'", rslt);
            return wasSysprepped;
        }

        private void SetupSubscriptions()
        {
            var agentSubscription = new BoshAgentSubscription(config.AgentID);
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

            // encryption code here if @credentials

            string method = (string)j["method"];

            if (method == "get_state")
            {
                method = "state";
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

            try
            {
                using (IMessageHandler handler = ioc.GetInstance<IMessageHandler>(method))
                {
                    HandlerResponse response = handler.Handle(j);
                    Publish(replyTo, response);
                    handler.OnPostReply();
                }
            }
            catch (StructureMapException ex)
            {
                log.Error(ex, Resources.BoshAgent_MissingHandlerForMethod_Fmt, method);
                return;
            }
            catch (Exception ex)
            {
                log.Error(ex, Resources.BoshAgent_ExceptionHandlingMethod_Fmt, method);
                var remoteException = RemoteException.From(ex);
                Publish(replyTo, remoteException);
            }
        }

        private void Publish(string replyTo, RemoteException exception)
        {
            // TODO UGLY!
            string blobstoreID = null;
            if (null != exception.Blob)
            {
                string tmpFile = Path.GetTempFileName();
                try
                {
                    File.WriteAllText(tmpFile, exception.Blob);
                    IBlobstoreClient bsc = blobstoreClientFactory.Create();
                    blobstoreID = bsc.Create(tmpFile);
                }
                finally
                {
                    if (File.Exists(tmpFile))
                    {
                        File.Delete(tmpFile);
                    }
                }
            }
            var pMessage = new JProperty("message", exception.Message);
            var pBacktrace = new JProperty("backtrace", exception.Backtrace);
            var pBlobstoreID = new JProperty("blobstore_id", blobstoreID);
            var jobj = new JObject(new JProperty("exception", new JObject(pMessage, pBacktrace, pBlobstoreID)));
            string json;
            using (var sw = new StringWriter())
            {
            	using (var writer = new JsonTextWriter(sw))
            	{
            		jobj.WriteTo(writer);
            	}
                json = sw.ToString();
            }
            Publish(replyTo, json);
        }

        private void Publish(string replyTo, HandlerResponse response)
        {
            string responseJsonStr = response.ToJson();
            Publish(replyTo, responseJsonStr);
        }

        private void Publish(string replyTo, string json)
        {
            const uint NATS_MAX_PAYLOAD_SIZE = 1024 * 1024;

            // TODO encrypt?

            int responseJsonSize = Encoding.ASCII.GetByteCount(json);

            log.Debug(Resources.BoshAgent_ResponseDebug_Fmt, replyTo, json);

            /*
              TODO figure out if we want to try to scale down the message instead of generating an exception
             */
            if (responseJsonSize < NATS_MAX_PAYLOAD_SIZE)
            {
                natsClient.PublishReply(replyTo, json, 0);
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

        // TODO move to Bootstrap class
        private void BoshAgentInfrastructureVsphereSettings_LoadSettings()
        {
            bool settingsFound = false;
            DirectoryInfo driveRootDirectory = null;

            settingsFound = LoadSettings();
            if (settingsFound)
            {
                return;
            }

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

            if (File.Exists(config.SettingsFilePath))
            {
                rv = LoadSettings(File.ReadAllText(config.SettingsFilePath));
            }

            return rv;
        }

        private bool LoadSettings(string settingsJsonStr)
        {
            settings = JObject.Parse(settingsJsonStr);

            if (null == settings["vm-setup"])
            {
                var setup = new VMSetupState { IsSysprepped = false, IsNetworkSetup = false };
                settings["vm-setup-state"] = JObject.FromObject(setup);
            }

            return true;
        }

        private void SaveSettings()
        {
            string settingsJsonStr = settings.ToString();
            File.WriteAllText(config.SettingsFilePath, settingsJsonStr);
        }

        private class BoshAgentNatsConfig : INatsConfig
        {
            private readonly string host;
            private readonly ushort port;
            private readonly string user;
            private readonly string password;

            public BoshAgentNatsConfig(Uri mbus)
            {
                // nats://nats:nats@172.21.10.181:4222
                this.host = mbus.Host;
                this.port = (ushort)mbus.Port;

                string[] userInfo = mbus.UserInfo.Split(new[] { ':' });
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