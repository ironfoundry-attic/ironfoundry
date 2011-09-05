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
    using CloudFoundry.Net.Dea.Entities;
    using CloudFoundry.Net.Dea.Providers;
    using CloudFoundry.Net.Dea.Providers.Interfaces;
    using ICSharpCode.SharpZipLib.GZip;
    using ICSharpCode.SharpZipLib.Tar;
    using NLog;

    public class Agent : IAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMessagingProvider NATS;
        private readonly IWebServerAdministrationProvider IIS;
        private readonly Dictionary<int, Dictionary<string, Instance>> Droplets = new Dictionary<int, Dictionary<string, Instance>>();        
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

            helloMessage = new Hello()
            {
                id = NATS.UniqueIdentifier,
                ip = Constants.LocalhostIP,
                port = 12345,
                version = 0.99
            };

            vcapComponentDiscoverMessage = new VcapComponentDiscover()
            {
                type = "DEA",
                index = 1,
                uuid = NATS.UniqueIdentifier,
                host = string.Format("{0}:{1}", Constants.LocalhostIP, 9999),
                credentials = NATS.UniqueIdentifier,
                start = DateTime.Now.ToString(Constants.JsonDateFormat)
            };

            snapshotFile = Path.Combine(ConfigurationManager.AppSettings[Constants.AppSettings.DropletsDirectory], "snapshot.json");
        }

        public void Start()
        {
            NATS.Connect();

            tasks.Add(Task.Factory.StartNew(NATS.Poll));

            NATS.Subscribe(Constants.Messages.VcapComponentDiscover, (msg,reply) => { });
            NATS.Publish(Constants.NatsCommands.Ok, vcapComponentDiscoverMessage.ToJson());
            NATS.Publish(Constants.Messages.VcapComponentAnnounce, vcapComponentDiscoverMessage.ToJson());
            NATS.Subscribe(Constants.Messages.DeaStatus, ProcessDeaStatus);
            NATS.Subscribe(Constants.Messages.DropletStatus, ProcessDropletStatus);
            NATS.Subscribe(Constants.Messages.DeaDiscover, ProcessDeaDiscover);
            NATS.Subscribe(Constants.Messages.DeaFindDroplet, ProcessDeaFindDroplet);
            NATS.Subscribe(Constants.Messages.DeaUpdate, ProcessDeaUpdate);
            NATS.Subscribe(Constants.Messages.DeaStop, ProcessDeaStop);
            NATS.Subscribe(String.Format(Constants.Messages.DeaInstanceStart, NATS.UniqueIdentifier), ProcessDeaStart);
            NATS.Subscribe(Constants.Messages.RouterStart, ProcessRouterStart);
            NATS.Subscribe(Constants.Messages.HealthManagerStart, ProcessHealthManagerStart);
            NATS.Publish(Constants.Messages.DeaStart, helloMessage.ToJson());

            recoverExistingDroplets();

            // Turn on Heartbeat Loop
            tasks.Add(Task.Factory.StartNew(HeartbeatsLoop));

            // TODO: Refactor ALL of the threading
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
                Thread.Sleep(10 * 1000);
            }
        }

        public void ProcessDeaStart(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);            
            
            Droplet droplet = message.FromJson<Droplet>();
            if (droplet.framework != "aspdotnet")
            {
                Logger.Debug("This DEA does not support non-aspdotnet frameworks");
                return;
            }

            var instance = new Instance()
            {
                droplet_id = droplet.droplet,
                instance_id = droplet.sha1,
                instance_index = droplet.index,
                name = droplet.name,
                dir = "/" + droplet.name,
                uris = droplet.uris,
                users = droplet.users,
                version = droplet.version,
                mem_quota = droplet.limits.mem * (1024*1024),
                disk_quota = droplet.limits.disk * (1024*1024),
                fds_quota = droplet.limits.fds,
                state = Constants.InstanceState.STARTING,
                runtime = droplet.runtime,
                framework = droplet.framework,
                start = DateTime.Now.ToString(Constants.JsonDateFormat),
                state_timestamp = Utility.GetEpochTimestamp(),
                log_id = string.Format("(name={0} app_id={1} instance={2} index={3})",droplet.name,droplet.droplet,droplet.sha1,droplet.index),                
                staged = droplet.name,
                sha1 = droplet.sha1
            };

            
            MemoryStream gzipMemoryStream = getStagedApplicationFile(droplet.executableUri);
            string dropletsPath = Path.Combine(ConfigurationManager.AppSettings[Constants.AppSettings.DropletsDirectory], instance.sha1);
            string applicationPath = Path.Combine(ConfigurationManager.AppSettings[Constants.AppSettings.ApplicationsDirectory], instance.sha1);
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
            WebServerAdministrationBinding binding = IIS.InstallWebApp(applicationPath, instance.sha1);
            instance.host = binding.Host;
            instance.port = binding.Port;

            registerWithRouter(instance, instance.uris);

            instance.state = Constants.InstanceState.STARTING;
            instance.state_timestamp = Utility.GetEpochTimestamp();
            sendSingleHeartbeat(generateHeartbeat(instance));
            
            Dictionary<string, Instance> instances;
            lock (lockObject)
            {
                instances = new Dictionary<string, Instance>();
                instances.Add(instance.instance_id, instance);
                Droplets.Add(droplet.droplet, instances);
            }
            takeSnapshot();
        }

        public void ProcessDeaUpdate(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            var droplet = message.FromJson<Droplet>();
            var instance = getInstance(droplet.droplet);
            string[] current_uris = new string[instance.uris.Length];
            Array.Copy(instance.uris, current_uris, instance.uris.Length);
            instance.uris = droplet.uris;

            var toRemove = current_uris.Except(droplet.uris);
            var toAdd = droplet.uris.Except(current_uris);
            
            unregisterWithRouter(instance, toRemove.ToArray());
            registerWithRouter(instance, toAdd.ToArray());
            takeSnapshot();
        }

        public void ProcessDeaDiscover(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            var discover = message.FromJson<DiscoverMessage>();
            NATS.Publish(reply, helloMessage.ToJson());
        }  
      
        public void ProcessDeaStop(string message, string reply)
        {            
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            var droplet = message.FromJson<Droplet>();
            var instance = getInstance(droplet.droplet);
            if (instance != null)
            {
                IIS.UninstallWebApp(instance.sha1);
                unregisterWithRouter(instance, instance.uris);
                Droplets.Remove(droplet.droplet);                
                takeSnapshot();
                NATS.Publish(Constants.NatsCommands.Ok, message);
            }
        }

        public void ProcessDeaStatus(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            Status statusMessage = new Status()
            {
                id = helloMessage.id,
                ip = helloMessage.ip,
                port = helloMessage.port,
                version = helloMessage.version,
                max_memory = 4096,
                used_memory = 0,
                reserved_memory = 0,
                num_clients = 20
            };
            NATS.Publish(reply, statusMessage.ToJson());
        }

        public void ProcessDeaFindDroplet(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            var findDroplet = message.FromJson<FindDroplet>();
            forAllInstances((instance) =>
            {
                if (instance.droplet_id == findDroplet.droplet)
                {
                    if (instance.version == findDroplet.version)
                    {
                        if (findDroplet.states.Contains(instance.state))
                        {
                            var startDate = DateTime.ParseExact(instance.start, Constants.JsonDateFormat, CultureInfo.InvariantCulture);
                            var span = DateTime.Now - startDate;
                            var response = new FindDropletResponse()
                            {
                                dea = NATS.UniqueIdentifier,
                                version = instance.version,
                                droplet = instance.droplet_id,
                                instance = instance.instance_id,
                                index = instance.instance_index,
                                state = instance.state,
                                state_timestamp = instance.state_timestamp,
                                file_uri = string.Empty,
                                credentials = string.Empty,
                                staged = instance.staged,
                                stats = new Stats()
                                {
                                    name = instance.name,
                                    host = instance.host,
                                    port = instance.port,
                                    uris = instance.uris,
                                    uptime = span.TotalSeconds,
                                    mem_quota = instance.mem_quota,
                                    disk_quota = instance.disk_quota,
                                    fds_quota = instance.fds_quota,
                                    cores = 1,
                                    usage = 20
                                }
                            };
                            if (response.state != Constants.InstanceState.RUNNING)
                                response.stats = null;
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
                var startDate = DateTime.ParseExact(instance.start,Constants.JsonDateFormat,CultureInfo.InvariantCulture);
                var span = DateTime.Now - startDate;
                var response = new Stats()
                {
                    name = instance.name,
                    host = instance.host,
                    port = instance.port,
                    uris = instance.uris,
                    uptime = span.TotalSeconds,
                    mem_quota = instance.mem_quota,
                    disk_quota = instance.disk_quota,
                    fds_quota = instance.fds_quota,
                    usage = 20
                };
                NATS.Publish(reply, message);
            });
        }

        public void ProcessRouterStart(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name,message);
            forAllInstances((instance) => registerWithRouter(instance,instance.uris));
        }

        public void ProcessHealthManagerStart(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            sendHeartbeat();
        }

        private string getApplicationState(string name)
        {
            if (!IIS.DoesApplicationExist(name))
                return Constants.InstanceState.DELETED;

            var status = IIS.GetStatus(name);
            switch (status)
            {
                case ApplicationInstanceStatus.Started:
                    return Constants.InstanceState.RUNNING;
                case ApplicationInstanceStatus.Starting:
                    return Constants.InstanceState.STARTING;
                case ApplicationInstanceStatus.Stopping:
                    return Constants.InstanceState.SHUTTING_DOWN;
                case ApplicationInstanceStatus.Stopped:
                    return Constants.InstanceState.STOPPED;
                case ApplicationInstanceStatus.Unknown:
                    return Constants.InstanceState.CRASHED;
                default:
                    return Constants.InstanceState.CRASHED;
            }
        }

        private MemoryStream getStagedApplicationFile(string executableUri)
        {
            MemoryStream returnStream = null;
            using (WebClient client = new WebClient())
                returnStream = new MemoryStream(client.DownloadData(executableUri));
            return returnStream;
        }

        private void registerWithRouter(Instance instance, string[] uris)
        {
            if (uris.Length == 0)
                return;
            var routerRegister = new RouterRegister()
            {
                dea = NATS.UniqueIdentifier,
                host = instance.host,
                port = instance.port,
                uris = uris,
                tags = new Tag() { framework = instance.framework, runtime = instance.runtime }
            };
            NATS.Publish(Constants.Messages.RouterRegister, routerRegister.ToJson());
        }

        private void unregisterWithRouter(Instance instance, string[] uris)
        {
            if (uris.Length == 0)
                return;
            var routerRegister = new RouterRegister()
            {
                dea = NATS.UniqueIdentifier,
                host = instance.host,
                port = instance.port,
                uris = uris
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
                instance.state = getApplicationState(instance.sha1);
                instance.state_timestamp = Utility.GetEpochTimestamp();
                heartbeats.Add(generateHeartbeat(instance));
            });

            var dropletHeartbeats = new
            {
                droplets = heartbeats.ToArray()
            };
            NATS.Publish(Constants.Messages.DeaHeartbeat, dropletHeartbeats.ToJson());
        }

        private void takeSnapshot()
        {
            var dropletEntries = new List<DropletEntry>();
            foreach(var droplet in Droplets)
            {
                var instanceEntries = new List<InstanceEntry>();
                foreach(var instance in droplet.Value)
                {
                    InstanceEntry i = new InstanceEntry() {
                        instance_id = instance.Key,
                        instance = instance.Value
                    };
                    instanceEntries.Add(i);
                }

                var d = new DropletEntry() {
                    droplet = droplet.Key,
                    instances = instanceEntries.ToArray()
                };
                dropletEntries.Add(d);
            }
            var snapshot = new Snapshot()
            {
                entries = dropletEntries.ToArray()
            };

            File.WriteAllText(snapshotFile,snapshot.ToJson(), new ASCIIEncoding());
        }

        private void recoverExistingDroplets()
        {
            if (File.Exists(snapshotFile))
            {
                string dropletsJson = File.ReadAllText(snapshotFile, new ASCIIEncoding());
                var snapshot = dropletsJson.FromJson<Snapshot>();
                foreach (var dropletEntry in snapshot.entries)
                    foreach (var instanceEntry in dropletEntry.instances)
                    {
                        var instances = new Dictionary<string, Instance>();
                        instances.Add(instanceEntry.instance_id, instanceEntry.instance);
                        Droplets.Add(dropletEntry.droplet, instances);
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

        private Instance getInstance(int dropletId)
        {
            if (!Droplets.Keys.Contains(dropletId))
                return null;
            return Droplets[dropletId].First().Value;
        }

        private void sendSingleHeartbeat(Heartbeat heartbeat)
        {
            var dropletHeartbeats = new
            {
                droplets = new Heartbeat[] { heartbeat }
            };
            NATS.Publish(Constants.Messages.DeaHeartbeat, dropletHeartbeats.ToJson());
        }

        private Heartbeat generateHeartbeat(Instance instance)
        {
            var heartbeat = new Heartbeat()
            {
                droplet = instance.droplet_id,
                version = instance.version,
                instance = instance.instance_id,
                index = instance.instance_index,
                state = instance.state,
                state_timestamp = instance.state_timestamp
            };

            return heartbeat;
        }
    }
}