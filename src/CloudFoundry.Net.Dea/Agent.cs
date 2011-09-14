namespace CloudFoundry.Net.Dea
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ICSharpCode.SharpZipLib.GZip;
    using ICSharpCode.SharpZipLib.Tar;
    using NLog;
    using Providers;
    using Providers.Interfaces;
    using Types;

    public sealed class Agent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMessagingProvider NATS;
        private readonly IWebServerAdministrationProvider IIS;
        private readonly IDictionary<uint, Dictionary<Guid, Instance>> Droplets = new Dictionary<uint, Dictionary<Guid, Instance>>();        
        private readonly Hello helloMessage;
        private readonly string snapshotFile;
        private readonly object lockObject = new object();
        private readonly IList<Task> tasks = new List<Task>();

        private bool shutting_down = false; // TODO: cancellation tokens

        public Agent()
        {
            var providerFactory = new ProviderFactory();

            NATS = providerFactory.CreateMessagingProvider(
                ConfigurationManager.AppSettings[Constants.AppSettings.NatsHost],
                Convert.ToUInt16(ConfigurationManager.AppSettings[Constants.AppSettings.NatsPort]));

            IIS = providerFactory.CreateWebServerAdministrationProvider();

            helloMessage = new Hello
            {
                ID = NATS.UniqueIdentifier,
                IPAddress = Utility.LocalIPAddress,
                Port = 12345,
                Version = 0.99M,
            };

            snapshotFile = Path.Combine(ConfigurationManager.AppSettings[Constants.AppSettings.DropletsDirectory], "snapshot.json");
        }

        public void Start()
        {
            NATS.Connect();

            tasks.Add(Task.Factory.StartNew(NATS.Start));

            // TODO do we have to wait for poll to start?

            NATS.Subscribe(Constants.Messages.VcapComponentDiscover, (msg, reply) => { }); // TODO subscribe to message types instead

            var vcapComponentDiscoverMessage = new VcapComponentDiscover
            {
                Type        = "DEA",
                Index       = 1,
                Uuid        = NATS.UniqueIdentifier,
                Host        = Utility.LocalIPAddress.ToString(),
                Credentials = NATS.UniqueIdentifier,
                Start       = DateTime.Now, // TODO UTC?
            };

            NATS.Publish(Constants.NatsCommands.Ok, vcapComponentDiscoverMessage);

            NATS.Publish(Constants.Messages.VcapComponentAnnounce, vcapComponentDiscoverMessage);

            NATS.Subscribe(Constants.Messages.DeaStatus, processDeaStatus);
            NATS.Subscribe(Constants.Messages.DropletStatus, processDropletStatus);
            NATS.Subscribe(Constants.Messages.DeaDiscover, processDeaDiscover);
            NATS.Subscribe(Constants.Messages.DeaFindDroplet, processDeaFindDroplet);
            NATS.Subscribe(Constants.Messages.DeaUpdate, processDeaUpdate);
            NATS.Subscribe(Constants.Messages.DeaStop, processDeaStop);
            NATS.Subscribe(String.Format(Constants.Messages.DeaInstanceStart, NATS.UniqueIdentifier), processDeaStart);
            NATS.Subscribe(Constants.Messages.RouterStart, processRouterStart);
            NATS.Subscribe(Constants.Messages.HealthManagerStart, processHealthManagerStart);

            NATS.Publish(Constants.Messages.DeaStart, helloMessage);

            recoverExistingDroplets();

            tasks.Add(Task.Factory.StartNew(heartbeatsLoop));

            // TODO refactor threading?
        }

        public void Stop()
        {
            // USING NATS to KILL threads
            shutting_down = true;
            NATS.Dispose();
            Task.WaitAll(tasks.ToArray(), TimeSpan.FromMinutes(1));
        }

        private void heartbeatsLoop()
        {
            while (false == shutting_down)
            {
                sendHeartbeat();
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }

        private void processDeaStart(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);            
            
            Droplet droplet = Message.FromJson<Droplet>(message);
            if (droplet.Framework != "aspdotnet")
            {
                Logger.Debug("This DEA does not support non-aspdotnet frameworks");
                return;
            }

            var instance = new Instance(droplet);
            
            FileData file = getStagedApplicationFile(droplet.ExecutableUri);
            if (null != file)
            {
                string dropletsPath = Path.Combine(ConfigurationManager.AppSettings[Constants.AppSettings.DropletsDirectory], instance.Dir);
                string applicationPath = Path.Combine(ConfigurationManager.AppSettings[Constants.AppSettings.ApplicationsDirectory], instance.Dir);
                Directory.CreateDirectory(dropletsPath);
                Directory.CreateDirectory(applicationPath);

                using (var gzipStream = new GZipInputStream(file.FileStream))
                {
                    TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                    tarArchive.ExtractContents(dropletsPath);
                    tarArchive.Close();
                }

                file.Dispose();

                if (false == shutting_down)
                {
                    Utility.CopyDirectory(new DirectoryInfo(dropletsPath + @"/app"), new DirectoryInfo(applicationPath));
                    WebServerAdministrationBinding binding = IIS.InstallWebApp(applicationPath, instance.IIsName);
                    instance.Host = binding.Host;
                    instance.Port = binding.Port;

                    instance.StateTimestamp = Utility.GetEpochTimestamp();

                    instance.State = Instance.InstanceState.RUNNING;
                    sendSingleHeartbeat(generateHeartbeat(instance));

                    registerWithRouter(instance, instance.Uris);

                    lock (lockObject)
                    {
                        Dictionary<Guid, Instance> instances;
                        if (Droplets.TryGetValue(droplet.ID, out instances))
                        {
                            instances.Add(instance.InstanceID, instance);
                        }
                        else
                        {
                            instances = new Dictionary<Guid, Instance>
                        {
                            { instance.InstanceID, instance }
                        };
                            Droplets.Add(droplet.ID, instances);
                        }
                    }

                    takeSnapshot();
                }
            }
        }

        /*
         * TODO UPDATE TO WORK WITH INSTANCES
         */
        private void processDeaUpdate(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            Droplet droplet = Message.FromJson<Droplet>(message);
            Instance instance = getFirstInstance(droplet.ID);
            string[] current_uris = new string[instance.Uris.Length];
            Array.Copy(instance.Uris, current_uris, instance.Uris.Length);
            instance.Uris = droplet.Uris;

            var toRemove = current_uris.Except(droplet.Uris);
            var toAdd = droplet.Uris.Except(current_uris);
            
            unregisterWithRouter(instance, toRemove.ToArray());

            registerWithRouter(instance, toAdd.ToArray());

            takeSnapshot();
        }

        private void processDeaDiscover(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            Discover discover = Message.FromJson<Discover>(message);
            NATS.Publish(reply, helloMessage);
        }  
      
        /*
         * TODO UPDATE TO WORK WITH INSTANCES
         */
        /*
    def process_dea_stop(message)
      return if @shutting_down
      message_json = JSON.parse(message)
      @logger.debug("DEA received stop message: #{message}")

      droplet_id   = message_json['droplet']
      version      = message_json['version']
      instance_ids = message_json['instances'] ? Set.new(message_json['instances']) : nil
      indices      = message_json['indices'] ? Set.new(message_json['indices']) : nil
      states       = message_json['states'] ? Set.new(message_json['states']) : nil

      return unless instances = @droplets[droplet_id]
      instances.each_value do |instance|
        version_matched  = version.nil? || instance[:version] == version
        instance_matched = instance_ids.nil? || instance_ids.include?(instance[:instance_id])
        index_matched    = indices.nil? || indices.include?(instance[:instance_index])
        state_matched    = states.nil? || states.include?(instance[:state].to_s)
        if (version_matched && instance_matched && index_matched && state_matched)
          instance[:exit_reason] = :STOPPED if [:STARTING, :RUNNING].include?(instance[:state])
          if instance[:state] == :CRASHED
            instance[:state] = :DELETED
            instance[:stop_processed] = false
          end
          stop_droplet(instance)
        end
      end
    end
         */
        private void processDeaStop(string message, string reply)
        {            
            if (shutting_down)
                return;

            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            StopDroplet stopDropletMsg = Message.FromJson<StopDroplet>(message);
            forAllInstances((instance) =>
                {
                    IIS.UninstallWebApp(instance.IIsName);
                    unregisterWithRouter(instance, instance.Uris);
                });
            // TODO stop_droplet() !!!
            Droplets.Remove(stopDropletMsg.ID); // TODO: lock??
            takeSnapshot();
            NATS.Publish(Constants.NatsCommands.Ok, message);
        }

        private void processDeaStatus(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            var statusMessage = new Status
            {
                ID             = helloMessage.ID,
                IPAddress      = helloMessage.IPAddress,
                Port           = helloMessage.Port,
                Version        = helloMessage.Version,
                MaxMemory      = 4096,
                UsedMemory     = 0,
                ReservedMemory = 0,
                NumClients     = 20
            };
            NATS.Publish(reply, statusMessage.ToJson());
        }

        private void processDeaFindDroplet(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            FindDroplet findDroplet = Message.FromJson<FindDroplet>(message);
            forAllInstances((instance) =>
            {
                if (instance.DropletID == findDroplet.DropletID)
                {
                    if (instance.Version == findDroplet.Version)
                    {
                        if (findDroplet.States.Contains(instance.State))
                        {
                            var startDate = DateTime.ParseExact(instance.Start, Constants.JsonDateFormat, CultureInfo.InvariantCulture);
                            var span = DateTime.Now - startDate;
                            var response = new FindDropletResponse // TODO ctor arg
                            {
                                Dea            = NATS.UniqueIdentifier,
                                Version        = instance.Version,
                                Droplet        = instance.DropletID,
                                InstanceID     = instance.InstanceID,
                                Index          = instance.InstanceIndex,
                                State          = instance.State,
                                StateTimestamp = instance.StateTimestamp,
                                FileUri        = string.Empty,
                                Credentials    = string.Empty,
                                Staged         = instance.Staged,
                                Stats          = new Stats()
                                {
                                    Name      = instance.Name,
                                    Host      = instance.Host,
                                    Port      = instance.Port,
                                    Uris      = instance.Uris,
                                    Uptime    = span.TotalSeconds,
                                    MemQuota  = instance.MemQuota,
                                    DiskQuota = instance.DiskQuota,
                                    FdsQuota  = instance.FdsQuota,
                                    Cores     = 1,
                                    Usage     = 20
                                }
                            };
                            if (response.State != Instance.InstanceState.RUNNING)
                                response.Stats = null;
                            NATS.Publish(reply, response.ToJson());
                        }
                    }
                }
            });
        }

        private void processDropletStatus(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            forAllInstances((instance) =>
            {
                if (instance.IsStarting || instance.IsRunning)
                {
                    var startDate = DateTime.ParseExact(instance.Start, Constants.JsonDateFormat, CultureInfo.InvariantCulture);
                    var span = DateTime.Now - startDate;
                    var response = new Stats(instance, span)
                    {
                        Usage = 20
                    };
                    NATS.Publish(reply, response);
                }
            });
        }

        private void processRouterStart(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name,message);

            forAllInstances((instance) =>
                {
                    if (instance.IsRunning)
                    {
                        registerWithRouter(instance, instance.Uris);
                    }
                });
        }

        private void processHealthManagerStart(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            sendHeartbeat();
        }

        private void registerWithRouter(Instance instance, string[] uris)
        {
            if (shutting_down || uris.IsNullOrEmpty())
                return;

            var routerRegister = new RouterRegister
            {
                Dea  = NATS.UniqueIdentifier,
                Host = instance.Host,
                Port = instance.Port,
                Uris = uris,
                Tag  = new Tag { Framework = instance.Framework, Runtime = instance.Runtime }
            };

            NATS.Publish(Constants.Messages.RouterRegister, routerRegister);
        }

        private void unregisterWithRouter(Instance instance, string[] uris)
        {
            if (shutting_down || uris.IsNullOrEmpty())
                return;

            var routerRegister = new RouterRegister
            {
                Dea  = NATS.UniqueIdentifier,
                Host = instance.Host,
                Port = instance.Port,
                Uris = uris
            };

            NATS.Publish(Constants.Messages.RouterUnregister, routerRegister);
        }

        private void sendHeartbeat()
        {
            if (shutting_down || Droplets.IsNullOrEmpty())
                return;

            var heartbeats = new List<Heartbeat>();

            forAllInstances((instance) =>
            {
                instance.State = getApplicationState(instance.IIsName);
                instance.StateTimestamp = Utility.GetEpochTimestamp();
                heartbeats.Add(generateHeartbeat(instance));
            });

            var message = new DropletHeartbeat
            {
                Droplets = heartbeats.ToArray()
            };

            NATS.Publish(Constants.Messages.DeaHeartbeat, message);
        }

        private string getApplicationState(string name)
        {
            if (!IIS.DoesApplicationExist(name))
                return Instance.InstanceState.DELETED;

            var status = IIS.GetStatus(name);
            switch (status)
            {
                case ApplicationInstanceStatus.Started:
                    return Instance.InstanceState.RUNNING;
                case ApplicationInstanceStatus.Starting:
                    return Instance.InstanceState.STARTING;
                case ApplicationInstanceStatus.Stopping:
                    return Instance.InstanceState.SHUTTING_DOWN;
                case ApplicationInstanceStatus.Stopped:
                    return Instance.InstanceState.STOPPED;
                case ApplicationInstanceStatus.Unknown:
                    return Instance.InstanceState.CRASHED;
                default:
                    return Instance.InstanceState.CRASHED;
            }
        }

        private void takeSnapshot()
        {
            var dropletEntries = new List<DropletEntry>();

            foreach (var droplet in Droplets)
            {
                var instanceEntries = new List<InstanceEntry>();

                foreach (var instance in droplet.Value)
                {
                    var instanceEntry = new InstanceEntry
                    {
                        InstanceID = instance.Key,
                        Instance = instance.Value
                    };
                    instanceEntries.Add(instanceEntry);
                }

                var d = new DropletEntry
                {
                    DropletID = droplet.Key,
                    Instances = instanceEntries.ToArray()
                };

                dropletEntries.Add(d);
            }

            var snapshot = new Snapshot()
            {
                Entries = dropletEntries.ToArray()
            };

            File.WriteAllText(snapshotFile, snapshot.ToJson(), new ASCIIEncoding());
        }

        private void recoverExistingDroplets()
        {
            if (File.Exists(snapshotFile))
            {
                lock (lockObject) // TODO: necessary?
                {
                    string dropletsJson = File.ReadAllText(snapshotFile, new ASCIIEncoding());
                    Snapshot snapshot = JsonBase.FromJson<Snapshot>(dropletsJson);
                    foreach (DropletEntry dropletEntry in snapshot.Entries)
                    {
                        foreach (InstanceEntry instanceEntry in dropletEntry.Instances)
                        {
                            /*
                             * TODO: this is where Ruby's auto-vivication is awesome.
                             */
                            Dictionary<Guid, Instance> instances;
                            if (Droplets.TryGetValue(dropletEntry.DropletID, out instances))
                            {
                                instances.Add(instanceEntry.InstanceID, instanceEntry.Instance);
                            }
                            else
                            {
                                instances = new Dictionary<Guid, Instance>
                                {
                                    { instanceEntry.InstanceID, instanceEntry.Instance }
                                };
                                Droplets.Add(dropletEntry.DropletID, instances);
                            }
                        }
                    }
                }
                sendHeartbeat();
                takeSnapshot();
            }
        }

        private void forAllInstances(Action<Instance> action)
        {
            lock (lockObject)
            {
                if (Droplets.IsNullOrEmpty())
                    return;

                foreach (Dictionary<Guid, Instance> droplet in Droplets.Values)
                {
                    foreach (Instance instance in droplet.Values)
                    {
                        action(instance);
                    }
                }
            }
        }

        private Instance getFirstInstance(uint dropletId)
        {
            if (!Droplets.Keys.Contains(dropletId))
                return null;
            return Droplets[dropletId].First().Value;
        }

        private void sendSingleHeartbeat(Heartbeat argHeartbeat)
        {
            var message = new DropletHeartbeat
            {
                Droplets = new[] { argHeartbeat }
            };
            NATS.Publish(Constants.Messages.DeaHeartbeat, message);
        }

        private static Heartbeat generateHeartbeat(Instance instance)
        {
            return new Heartbeat(instance);
        }

        private static FileData getStagedApplicationFile(string executableUri)
        {
            FileData rv = null;

            try
            {
                string tempFile = Path.GetTempFileName();

                var sw = new Stopwatch();
                sw.Start();
                using (var client = new WebClient())
                {
                    client.Proxy = null;
                    client.UseDefaultCredentials = false;
                    client.DownloadFile(executableUri, tempFile);
                }
                sw.Stop();
                Logger.Debug("Took {0} time to dowload from {1} to {2}", sw.Elapsed, executableUri, tempFile);

                rv = new FileData(new FileStream(tempFile, FileMode.Open), tempFile);
            }
            catch
            {
                // TODO
                // Can happen if there's a 404 or something.
            }

            return rv;
        }
    }
}