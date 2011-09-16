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
    using Properties;
    using Providers;
    using Types;

    public sealed class Agent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan HEARTBEAT_INTERVAL = TimeSpan.FromSeconds(10);

        private readonly IMessagingProvider NATS;
        private readonly IWebServerAdministrationProvider IIS;

        private readonly IDictionary<uint, IDictionary<Guid, Instance>> Droplets = new Dictionary<uint, IDictionary<Guid, Instance>>();

        private readonly Hello helloMessage;
        private readonly object lockObject = new object();
        private readonly IList<Task> tasks = new List<Task>();

        private bool shutting_down = false;

        private readonly string snapshotFile;

        public Agent()
        {
            var providerFactory = new ProviderFactory();

            NATS = providerFactory.CreateMessagingProvider(
                ConfigurationManager.AppSettings[Constants.AppSettings.NatsHost],
                Convert.ToUInt16(ConfigurationManager.AppSettings[Constants.AppSettings.NatsPort]));

            IIS = providerFactory.CreateWebServerAdministrationProvider();

            helloMessage = new Hello(NATS.UniqueIdentifier, Utility.LocalIPAddress, 12345, 0.99M);

            string dropletsPath = ConfigurationManager.AppSettings[Constants.AppSettings.DropletsDirectory];
            string applicationPath = ConfigurationManager.AppSettings[Constants.AppSettings.ApplicationsDirectory];
            Directory.CreateDirectory(dropletsPath);
            Directory.CreateDirectory(applicationPath);

            snapshotFile = Path.Combine(ConfigurationManager.AppSettings[Constants.AppSettings.DropletsDirectory], "snapshot.json");
        }

        public bool Error { get; private set; }

        public bool Start()
        {
            bool rv = false;

            if (NATS.Connect())
            {
                tasks.Add(Task.Factory.StartNew(NATS.Start));

                // TODO do we have to wait for poll to start?

                var vcapComponentDiscoverMessage = new VcapComponentDiscover(
                    argType: "DEA",
                    argIndex: 1,
                    argUuid: NATS.UniqueIdentifier, 
                    argHost: Utility.LocalIPAddress.ToString(),
                    argCredentials: NATS.UniqueIdentifier,
                    argStart: DateTime.Now);

                NATS.Subscribe(vcapComponentDiscoverMessage.PublishSubject,
                    (msg, reply) =>
                    {
                        // TODO update_discover_uptime
                        NATS.Publish(reply, vcapComponentDiscoverMessage);
                    });

                // TODO not necessary NATS.Publish(NatsCommand.Ok, vcapComponentDiscoverMessage);

                NATS.Publish( new VcapComponentAnnounce(vcapComponentDiscoverMessage));

                NATS.Subscribe(Message.Subjects.DeaStatus, processDeaStatus);
                NATS.Subscribe(Message.Subjects.DropletStatus, processDropletStatus);
                NATS.Subscribe(Message.Subjects.DeaDiscover, processDeaDiscover);
                NATS.Subscribe(Message.Subjects.DeaFindDroplet, processDeaFindDroplet);
                NATS.Subscribe(Message.Subjects.DeaUpdate, processDeaUpdate);
                NATS.Subscribe(Message.Subjects.DeaStop, processDeaStop);
                NATS.Subscribe(String.Format(Message.Subjects.DeaInstanceStart, NATS.UniqueIdentifier), processDeaStart);
                NATS.Subscribe(Message.Subjects.RouterStart, processRouterStart);
                NATS.Subscribe(Message.Subjects.HealthManagerStart, processHealthManagerStart);

                NATS.Publish(helloMessage);

                recoverExistingDroplets();

                tasks.Add(Task.Factory.StartNew(monitorLoop));

                // TODO refactor threading?
                // TODO how to indicate problems in threads? Events?
                rv = true;
            }

            return rv;
        }

        public void Stop() // evacuate_apps_then_quit TODO
        {
            if (shutting_down)
                return;

            shutting_down = true;

            Logger.Info("Evacuating applications..");

            forAllInstances(
                (dropletID) =>
                {
                    Logger.Debug("Evacuating app {0}", dropletID);
                },
                (instance) =>
                {
                    if (instance.IsCrashed)
                        return;

                    instance.DeaEvacuation();

                    sendExitedNotification(instance);
                });

            takeSnapshot();

            NATS.Dispose();

            Logger.Debug(Resources.Agent_WaitingForTasksInStop_Message);

            Task.WaitAll(tasks.ToArray(), TimeSpan.FromMinutes(1));

            Logger.Debug(Resources.Agent_TasksCompletedInStop_Message);
        }

        private void monitorLoop()
        {
            while (false == shutting_down)
            {
                if (NatsMessagingStatus.RUNNING != NATS.Status)
                {
                    Logger.Error(Resources.Agent_ErrorDetectedInNats_Message);
                    Error = true;
                    return;
                }
                else
                {
                    sendHeartbeat();
                }
                Thread.Sleep(HEARTBEAT_INTERVAL);
            }
        }

        private void processDeaStart(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting processDeaStart: {0}", message);

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
                        IDictionary<Guid, Instance> instances;
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

            Logger.Debug("Starting processDeaUpdate: {0}", message);
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

            Logger.Debug("Starting processDeaDiscover: {0}", message);
            Discover discover = Message.FromJson<Discover>(message);
            NATS.Publish(reply, helloMessage);
        }

        /*
         * stop_droplet
         * cleanup_droplet
         */
        private void processDeaStop(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting processDeaStop: {0}", message);

            StopDroplet stopDropletMsg = Message.FromJson<StopDroplet>(message);
            forAllInstances((argInstance) =>
                {
                    // TODO version and instance matching
                    IIS.UninstallWebApp(argInstance.IIsName);

                    unregisterWithRouter(argInstance, argInstance.Uris);

                    if (argInstance.StopProcessed)
                        return;

                    sendExitedMessage(argInstance);

                    Logger.Info("Stopping instance {0}", argInstance.LogID);

                    argInstance.StopProcessed = true;
                });

            // TODO delete files?
            takeSnapshot();
        }

        private void sendExitedMessage(Instance argInstance)
        {
            if (argInstance.IsNotified)
                return;

            unregisterWithRouter(argInstance);

            if (false == argInstance.HasExitReason)
            {
                argInstance.Crashed();
            }

            sendExitedNotification(argInstance);

            argInstance.IsNotified = true;
        }

        private void processDeaStatus(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting processDeaStatus: {0}", message);
            var statusMessage = new Status(helloMessage)
            {
                MaxMemory      = 4096,
                UsedMemory     = 0,
                ReservedMemory = 0,
                NumClients     = 20
            };
            NATS.Publish(reply, statusMessage);
        }

        private void processDeaFindDroplet(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting processDeaFindDroplet: {0}", message);
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
                                Stats          = new Stats
                                {
                                    Name      = instance.Name,
                                    Host      = instance.Host,
                                    Port      = instance.Port,
                                    Uris      = instance.Uris,
                                    Uptime    = span.TotalSeconds,
                                    MemQuota  = instance.MemQuota,
                                    DiskQuota = instance.DiskQuota,
                                    FdsQuota  = instance.FdsQuota,
                                    Cores     = 1
                                    //,Usage     = 20
                                }
                            };

                            if (response.State != Instance.InstanceState.RUNNING)
                            {
                                response.Stats = null;
                            }

                            NATS.Publish(reply, response);
                        }
                    }
                }
            });
        }

        private void processDropletStatus(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting processDropletStatus: {0}", message);
            forAllInstances((instance) =>
            {
                if (instance.IsStarting || instance.IsRunning)
                {
                    var startDate = DateTime.ParseExact(instance.Start, Constants.JsonDateFormat, CultureInfo.InvariantCulture);
                    var span = DateTime.Now - startDate;
                    var response = new Stats(instance, span)
                    {
                        //Usage = 20
                    };
                    NATS.Publish(reply, response);
                }
            });
        }

        private void processRouterStart(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting processRouterStart: {0}", message);

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
            Logger.Debug("Starting processHealthManagerStart: {0}", message);
            sendHeartbeat();
        }

        private void registerWithRouter(Instance argInstance, string[] argUris)
        {
            if (shutting_down || argInstance.Uris.IsNullOrEmpty())
                return;

            var routerRegister = new RouterRegister
            {
                Dea  = NATS.UniqueIdentifier,
                Host = argInstance.Host,
                Port = argInstance.Port,
                Uris = argUris ?? argInstance.Uris,
                Tag  = new Tag { Framework = argInstance.Framework, Runtime = argInstance.Runtime }
            };

            NATS.Publish(routerRegister);
        }

        private void unregisterWithRouter(Instance argInstance)
        {
            unregisterWithRouter(argInstance, null);
        }

        private void unregisterWithRouter(Instance argInstance, string[] argUris)
        {
            if (shutting_down || argInstance.Uris.IsNullOrEmpty())
                return;

            var routerUnregister = new RouterUnregister
            {
                Dea  = NATS.UniqueIdentifier,
                Host = argInstance.Host,
                Port = argInstance.Port,
                Uris = argUris ?? argInstance.Uris,
            };

            NATS.Publish(routerUnregister);
        }

        private void sendExitedNotification(Instance argInstance)
        {
            if (argInstance.IsEvacuated)
                return;

            NATS.Publish(new InstanceExited(argInstance));

            Logger.Debug("Sent droplet.exited.");
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

            NATS.Publish( message);
        }

        private string getApplicationState(string name)
        {
            if (false == IIS.DoesApplicationExist(name))
                return Instance.InstanceState.DELETED;

            ApplicationInstanceStatus status = IIS.GetStatus(name);

            string rv;

            switch (status)
            {
                case ApplicationInstanceStatus.Started:
                    rv = Instance.InstanceState.RUNNING;
                    break;
                case ApplicationInstanceStatus.Starting:
                    rv = Instance.InstanceState.STARTING;
                    break;
                case ApplicationInstanceStatus.Stopping:
                    rv = Instance.InstanceState.SHUTTING_DOWN;
                    break;
                case ApplicationInstanceStatus.Stopped:
                    rv = Instance.InstanceState.STOPPED;
                    break;
                case ApplicationInstanceStatus.Unknown:
                    rv = Instance.InstanceState.CRASHED;
                    break;
                default:
                    rv = Instance.InstanceState.CRASHED;
                    break;
            }

            return rv;
        }

        private void takeSnapshot()
        {
            /*
             * TODO: should we bother with scheduling?
             */
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
                            IDictionary<Guid, Instance> instances;
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

        private void forAllInstances(Action<Instance> argInstanceAction)
        {
            forAllInstances(null, argInstanceAction);
        }

        private void forAllInstances(Action<uint> argDropletAction, Action<Instance> argInstanceAction)
        {
            lock (lockObject)
            {
                if (Droplets.IsNullOrEmpty())
                    return;

                foreach (KeyValuePair<uint, IDictionary<Guid, Instance>> kvp in Droplets)
                {
                    uint dropletID = kvp.Key;
                    IDictionary<Guid, Instance> instanceDict = kvp.Value;

                    if (null != argDropletAction)
                    {
                        argDropletAction(dropletID);
                    }

                    foreach (Instance instance in instanceDict.Values)
                    {
                        argInstanceAction(instance);
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
            NATS.Publish(message);
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
