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

    public class Agent : IAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMessagingProvider NATS;
        private readonly IWebServerAdministrationProvider IIS;
        private readonly Dictionary<uint, Dictionary<string, Instance>> Droplets = new Dictionary<uint, Dictionary<string, Instance>>();        
        private readonly Hello helloMessage;
        private readonly VcapComponentDiscover vcapComponentDiscoverMessage;
        private readonly string IISHost;
        private readonly string snapshotFile;
        private readonly object lockObject = new object();
        private readonly IList<Task> tasks = new List<Task>();

        private bool stopping = false; // TODO: cancellation tokens

        public Agent()
        {
            var providerFactory = new ProviderFactory();

            NATS = providerFactory.CreateMessagingProvider(
                ConfigurationManager.AppSettings[Constants.AppSettings.NatsHost],
                Convert.ToInt32(ConfigurationManager.AppSettings[Constants.AppSettings.NatsPort]));

            IIS = providerFactory.CreateWebServerAdministrationProvider();

            IISHost = ConfigurationManager.AppSettings[Constants.AppSettings.IISHost];

            helloMessage = new Hello
            {
                ID = NATS.UniqueIdentifier,
                IPAddress = Constants.LocalhostIP,
                Port = 12345,
                Version = 0.99M,
            };

            vcapComponentDiscoverMessage = new VcapComponentDiscover
            {
                Type        = "DEA",
                Index       = 1,
                Uuid        = NATS.UniqueIdentifier,
                Host        = String.Format("{0}:{1}", Constants.LocalhostIP, 9999), // TODO
                Credentials = NATS.UniqueIdentifier,
                Start       = DateTime.Now, // TODO .ToString(Constants.JsonDateFormat)
            };

            snapshotFile = Path.Combine(ConfigurationManager.AppSettings[Constants.AppSettings.DropletsDirectory], "snapshot.json");
        }

        public void Start()
        {
            NATS.Connect();

            tasks.Add(Task.Factory.StartNew(NATS.Poll));

            // TODO do we have to wait for poll to start?

            NATS.Subscribe(Constants.Messages.VcapComponentDiscover, (msg, reply) => { }); // TODO subscribe to message types instead

            NATS.Publish(Constants.NatsCommands.Ok, vcapComponentDiscoverMessage);

            NATS.Publish(Constants.Messages.VcapComponentAnnounce, vcapComponentDiscoverMessage);

            NATS.Subscribe(Constants.Messages.DeaStatus, ProcessDeaStatus);
            NATS.Subscribe(Constants.Messages.DropletStatus, ProcessDropletStatus);
            NATS.Subscribe(Constants.Messages.DeaDiscover, ProcessDeaDiscover);
            NATS.Subscribe(Constants.Messages.DeaFindDroplet, ProcessDeaFindDroplet);
            NATS.Subscribe(Constants.Messages.DeaUpdate, ProcessDeaUpdate);
            NATS.Subscribe(Constants.Messages.DeaStop, ProcessDeaStop);
            NATS.Subscribe(String.Format(Constants.Messages.DeaInstanceStart, NATS.UniqueIdentifier), ProcessDeaStart);
            NATS.Subscribe(Constants.Messages.RouterStart, ProcessRouterStart);
            NATS.Subscribe(Constants.Messages.HealthManagerStart, ProcessHealthManagerStart);

            NATS.Publish(Constants.Messages.DeaStart, helloMessage);

            recoverExistingDroplets();

            tasks.Add(Task.Factory.StartNew(HeartbeatsLoop));

            // TODO refactor threading?
        }

        public void Stop()
        {
            // USING NATS to KILL threads
            stopping = true;
            NATS.Dispose();
            Task.WaitAll(tasks.ToArray(), TimeSpan.FromMinutes(1));
        }

        public void HeartbeatsLoop()
        {
            while (false == stopping)
            {
                sendHeartbeat();
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }

        public void ProcessDeaStart(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);            
            
            Droplet droplet = Message.FromJson<Droplet>(message);
            if (droplet.Framework != "aspdotnet")
            {
                Logger.Debug("This DEA does not support non-aspdotnet frameworks");
                return;
            }

            var instance = new Instance(droplet)
            {
                DropletID       = droplet.ID,
                InstanceID      = droplet.Sha1,
                InstanceIndex   = droplet.Index,
                Name            = droplet.Name,
                Dir             = "/" + droplet.Name,
                Uris            = droplet.Uris,
                Users           = droplet.Users,
                Version         = droplet.Version,
                MemQuota        = droplet.Limits.Mem * (1024*1024),
                DiskQuota       = droplet.Limits.Disk * (1024*1024),
                FdsQuota        = droplet.Limits.FDs,
                State           = Instance.InstanceState.STARTING,
                Runtime         = droplet.Runtime,
                Framework       = droplet.Framework,
                Start           = DateTime.Now.ToString(Constants.JsonDateFormat),
                StateTimestamp  = Utility.GetEpochTimestamp(),
                LogID           = String.Format("(name={0} app_id={1} instance={2} index={3})",droplet.Name,droplet.ID,droplet.Sha1,droplet.Index),                
                Staged          = droplet.Name,
                Sha1            = droplet.Sha1
            };

            
            MemoryStream gzipMemoryStream = getStagedApplicationFile(droplet.ExecutableUri);
            if (null != gzipMemoryStream)
            {
                string dropletsPath = Path.Combine(ConfigurationManager.AppSettings[Constants.AppSettings.DropletsDirectory], instance.Sha1);
                string applicationPath = Path.Combine(ConfigurationManager.AppSettings[Constants.AppSettings.ApplicationsDirectory], instance.Sha1);
                Directory.CreateDirectory(dropletsPath);
                Directory.CreateDirectory(applicationPath);

                byte[] dataBuffer = new byte[4096];
                using (GZipInputStream gzipStream = new GZipInputStream(gzipMemoryStream))
                {
                    TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                    tarArchive.ExtractContents(dropletsPath);
                    tarArchive.Close();
                }
                Utility.CopyDirectory(new DirectoryInfo(dropletsPath + @"/app"), new DirectoryInfo(applicationPath));
                WebServerAdministrationBinding binding = IIS.InstallWebApp(applicationPath, instance.Sha1);
                instance.Host = binding.Host;
                instance.Port = binding.Port;

                registerWithRouter(instance, instance.Uris);

                instance.State = Instance.InstanceState.STARTING;
                instance.StateTimestamp = Utility.GetEpochTimestamp();
                sendSingleHeartbeat(generateHeartbeat(instance));

                Dictionary<string, Instance> instances;
                lock (lockObject)
                {
                    instances = new Dictionary<string, Instance>();
                    instances.Add(instance.InstanceID, instance);
                    Droplets.Add(droplet.ID, instances);
                }
                takeSnapshot();
            }
        }

        public void ProcessDeaUpdate(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            Droplet droplet = Message.FromJson<Droplet>(message);
            var instance = getInstance(droplet.ID);
            string[] current_uris = new string[instance.Uris.Length];
            Array.Copy(instance.Uris, current_uris, instance.Uris.Length);
            instance.Uris = droplet.Uris;

            var toRemove = current_uris.Except(droplet.Uris);
            var toAdd = droplet.Uris.Except(current_uris);
            
            unregisterWithRouter(instance, toRemove.ToArray());
            registerWithRouter(instance, toAdd.ToArray());
            takeSnapshot();
        }

        public void ProcessDeaDiscover(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            Discover discover = Message.FromJson<Discover>(message);
            NATS.Publish(reply, helloMessage.ToJson());
        }  
      
        public void ProcessDeaStop(string message, string reply)
        {            
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            Droplet droplet = Message.FromJson<Droplet>(message);
            var instance = getInstance(droplet.ID);
            if (instance != null)
            {
                IIS.UninstallWebApp(instance.Sha1);
                unregisterWithRouter(instance, instance.Uris);
                Droplets.Remove(droplet.ID);                
                takeSnapshot();
                NATS.Publish(Constants.NatsCommands.Ok, message);
            }
        }

        public void ProcessDeaStatus(string message, string reply)
        {
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

        public void ProcessDeaFindDroplet(string message, string reply)
        {
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
                            var response = new FindDropletResponse()
                            {
                                Dea            = NATS.UniqueIdentifier,
                                Version        = instance.Version,
                                Droplet        = instance.DropletID,
                                Instance       = instance.InstanceID,
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

        public void ProcessDropletStatus(string message, string reply)
        {
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

        public void ProcessRouterStart(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name,message);
            forAllInstances((instance) => registerWithRouter(instance, instance.Uris));
        }

        public void ProcessHealthManagerStart(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            sendHeartbeat();
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

        private static MemoryStream getStagedApplicationFile(string executableUri)
        {
            MemoryStream returnStream = null;

            try
            {
                using (WebClient client = new WebClient())
                {
                    /*
                     * TODO: probably shouldn't download into memory
                     */
                    returnStream = new MemoryStream(client.DownloadData(executableUri));
                }
            }
            catch
            {
                // TODO
                // Can happen if there's a 404 or something.
            }
            return returnStream;
        }

        private void registerWithRouter(Instance instance, string[] uris)
        {
            if (uris.IsNullOrEmpty())
                return;

            var routerRegister = new RouterRegister
            {
                Dea  = NATS.UniqueIdentifier,
                Host = instance.Host,
                Port = instance.Port,
                Uris = uris,
                Tag  = new Tag { Framework = instance.Framework, Runtime = instance.Runtime }
            };

            NATS.Publish(Constants.Messages.RouterRegister, routerRegister.ToJson());
        }

        private void unregisterWithRouter(Instance instance, string[] uris)
        {
            if (uris.Length == 0)
                return;
            var routerRegister = new RouterRegister
            {
                Dea  = NATS.UniqueIdentifier,
                Host = instance.Host,
                Port = instance.Port,
                Uris = uris
            };
            NATS.Publish(Constants.Messages.RouterUnregister, routerRegister.ToJson());
        }

        private void sendHeartbeat()
        {
            if (Droplets.Count == 0)
                return;

            var heartbeats = new List<Heartbeat>();

            forAllInstances((instance) =>
            {
                instance.State = getApplicationState(instance.Sha1);
                instance.StateTimestamp = Utility.GetEpochTimestamp();
                heartbeats.Add(generateHeartbeat(instance));
            });

            var message = new DropletHeartbeat
            {
                Droplets = heartbeats.ToArray()
            };

            NATS.Publish(Constants.Messages.DeaHeartbeat, message);
        }

        private void takeSnapshot()
        {
            var dropletEntries = new List<DropletEntry>();
            foreach(var droplet in Droplets)
            {
                var instanceEntries = new List<InstanceEntry>();

                foreach(var instance in droplet.Value)
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
                    Droplet = droplet.Key,
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
                string dropletsJson = File.ReadAllText(snapshotFile, new ASCIIEncoding());
                Snapshot snapshot = JsonBase.FromJson<Snapshot>(dropletsJson);
                foreach (var dropletEntry in snapshot.Entries)
                    foreach (var instanceEntry in dropletEntry.Instances)
                    {
                        var instances = new Dictionary<string, Instance>();
                        instances.Add(instanceEntry.InstanceID, instanceEntry.Instance);
                        Droplets.Add(dropletEntry.Droplet, instances);
                    }
                sendHeartbeat();
                takeSnapshot();
            }
        }

        private void forAllInstances(Action<Instance> action)
        {
            lock (lockObject)
            {
                if (Droplets.Count == 0)
                    return;
                foreach (Dictionary<string, Instance> droplet in Droplets.Values)
                    foreach (Instance instance in droplet.Values)
                        action(instance);
            }
        }

        private Instance getInstance(uint dropletId)
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

        private Heartbeat generateHeartbeat(Instance instance)
        {
            return new Heartbeat // TODO instance ctor arg
            {
                Droplet        = instance.DropletID,
                Version        = instance.Version,
                Instance       = instance.InstanceID,
                Index          = instance.InstanceIndex,
                State          = instance.State,
                StateTimestamp = instance.StateTimestamp
            };
        }
    }
}