using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Dea.Providers.Interfaces;
using CloudFoundry.Net.Dea.Providers;
using System.Threading.Tasks;
using NLog;
using System.Diagnostics;
using System.Runtime.Serialization;
using CloudFoundry.Net.Dea.Entities;
using System.Threading;
using System.Net;
using System.Configuration;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using System.Globalization;

namespace CloudFoundry.Net.Dea
{
    public class Agent : IAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IMessagingProvider NATS;
        private IWebServerAdministrationProvider IIS;
        private Dictionary<int, Dictionary<string, Instance>> Droplets = new Dictionary<int, Dictionary<string, Instance>>();        
        private Hello helloMessage;
        private VcapComponentDiscover vcapComponentDiscoverMessage;
        private string IISHost;
        private string snapshotFile;
        private Object lockObject = new object();

        public Agent()
        {
            var providerFactory = new ProviderFactory();
            NATS = providerFactory.CreateMessagingProvider(ConfigurationManager.AppSettings[Constants.AppSettings.NatsHost],
                                                           Convert.ToInt32(ConfigurationManager.AppSettings[Constants.AppSettings.NatsPort]));
            IIS = providerFactory.CreateWebServerAdministrationProvider();
            Initialize();            
        }

        private void Initialize()
        {
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

        public void Run()
        {
            NATS.Connect();
            Task.Factory.StartNew(NATS.Poll);
            NATS.Subscribe(Constants.Messages.VcapComponentDiscover, (msg,reply) => { });
            NATS.Publish(Constants.NatsCommands.Ok, vcapComponentDiscoverMessage.ToJson());
            NATS.Publish(Constants.Messages.VcapComponentAnnounce, vcapComponentDiscoverMessage.ToJson());
            NATS.Subscribe(Constants.Messages.DeaStatus, ProcessDeaStatus);
            NATS.Subscribe(Constants.Messages.DropletStatus, ProcessDropletStatus);
            NATS.Subscribe(Constants.Messages.DeaDiscover, ProcessDeaDiscover);
            NATS.Subscribe(Constants.Messages.DeaFindDroplet, ProcessDeaFindDroplet);
            NATS.Subscribe(Constants.Messages.DeaUpdate, ProcessDeaUpdate);
            NATS.Subscribe(Constants.Messages.DeaStop, ProcessDeaStop);
            NATS.Subscribe(string.Format(Constants.Messages.DeaInstanceStart, NATS.UniqueIdentifier), ProcessDeaStart);
            NATS.Subscribe(Constants.Messages.RouterStart, ProcessRouterStart);
            NATS.Subscribe(Constants.Messages.HealthManagerStart, ProcessHealthManagerStart);
            NATS.Publish(Constants.Messages.DeaStart, helloMessage.ToJson());

            RecoverExistingDroplets();
            // Turn on Heartbeat Loop
            Task.Factory.StartNew(HeartbeatsLoop);

            // Currently we're running as a program
            // This is the kill method 
            // TODO: Refactor ALL of the threading
            Console.ReadLine();

            // USING NATS to KILL threads
            NATS.Dispose();
        }

        public void HeartbeatsLoop()
        {
            while (true)
            {
                SendHeartbeat();
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
                state_timestamp = Utility.GetEnochTimestamp(),
                log_id = string.Format("(name={0} app_id={1} instance={2} index={3})",droplet.name,droplet.droplet,droplet.sha1,droplet.index),                
                staged = droplet.name,
                sha1 = droplet.sha1
            };

            
            var gzipMemoryStream = GetStagedApplicationFile(droplet.executableUri);
            var dropletsPath = ConfigurationManager.AppSettings[Constants.AppSettings.DropletsDirectory] + @"\" + instance.sha1;
            var applicationPath = ConfigurationManager.AppSettings[Constants.AppSettings.ApplicationsDirectory] + @"\" + instance.sha1;
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
            var binding = IIS.InstallWebApp(applicationPath, instance.sha1);
            instance.host = binding.Host;
            instance.port = binding.Port;

            RegisterWithRouter(instance, instance.uris);

            instance.state = Constants.InstanceState.STARTING;
            instance.state_timestamp = Utility.GetEnochTimestamp();
            SendSingleHeartbeat(GenerateHeartbeat(instance));
            
            Dictionary<string, Instance> instances;
            lock (lockObject)
            {
                instances = new Dictionary<string, Instance>();
                instances.Add(instance.instance_id, instance);
                Droplets.Add(droplet.droplet, instances);
            }
            TakeSnapshot();
        }

        public void ProcessDeaUpdate(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            var droplet = message.FromJson<Droplet>();
            var instance = GetInstance(droplet.droplet);
            string[] current_uris = new string[instance.uris.Length];
            Array.Copy(instance.uris, current_uris, instance.uris.Length);
            instance.uris = droplet.uris;

            var toRemove = current_uris.Except(droplet.uris);
            var toAdd = droplet.uris.Except(current_uris);
            
            UnregisterWithRouter(instance, toRemove.ToArray());
            RegisterWithRouter(instance, toAdd.ToArray());
            TakeSnapshot();
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
            var instance = GetInstance(droplet.droplet);
            if (instance != null)
            {
                IIS.UninstallWebApp(instance.sha1);
                UnregisterWithRouter(instance, instance.uris);
                Droplets.Remove(droplet.droplet);                
                TakeSnapshot();
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
            ForAllInstances((instance) =>
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
            ForAllInstances((instance) =>
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
            ForAllInstances((instance) => RegisterWithRouter(instance,instance.uris));
        }

        public void ProcessHealthManagerStart(string message, string reply)
        {
            Logger.Debug("Starting {0}: {1}", new StackFrame(0).GetMethod().Name, message);
            SendHeartbeat();
        }

        private string GetApplicationState(string name)
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

        private MemoryStream GetStagedApplicationFile(string executableUri)
        {
            MemoryStream returnStream = null;
            using (WebClient client = new WebClient())
                returnStream = new MemoryStream(client.DownloadData(executableUri));
            return returnStream;
        }

        private void RegisterWithRouter(Instance instance, string[] uris)
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

        private void UnregisterWithRouter(Instance instance, string[] uris)
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

        private void SendHeartbeat()
        {
            if (Droplets.Count == 0)
                return;

            List<Heartbeat> heartbeats = new List<Heartbeat>();

            ForAllInstances((instance) =>
            {
                instance.state = GetApplicationState(instance.sha1);
                instance.state_timestamp = Utility.GetEnochTimestamp();
                heartbeats.Add(GenerateHeartbeat(instance));
            });

            var dropletHeartbeats = new
            {
                droplets = heartbeats.ToArray()
            };
            NATS.Publish(Constants.Messages.DeaHeartbeat, dropletHeartbeats.ToJson());
        }

        private void TakeSnapshot()
        {
            List<DropletEntry> dropletEntries = new List<DropletEntry>();
            foreach(var droplet in Droplets)
            {
                List<InstanceEntry> instanceEntries = new List<InstanceEntry>();
                foreach(var instance in droplet.Value)
                {
                    InstanceEntry i = new InstanceEntry() {
                        instance_id = instance.Key,
                        instance = instance.Value
                    };
                    instanceEntries.Add(i);
                }

                DropletEntry d = new DropletEntry() {
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

        private void RecoverExistingDroplets()
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
                SendHeartbeat();
                TakeSnapshot();
            }
        }

        private void ForAllInstances(Action<Instance> action)
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

        private Instance GetInstance(int dropletId)
        {
            if (!Droplets.Keys.Contains(dropletId))
                return null;
            return Droplets[dropletId].First().Value;
        }

        private void SendSingleHeartbeat(Heartbeat heartbeat)
        {
            var dropletHeartbeats = new
            {
                droplets = new Heartbeat[] { heartbeat }
            };
            NATS.Publish(Constants.Messages.DeaHeartbeat, dropletHeartbeats.ToJson());
        }

        private Heartbeat GenerateHeartbeat(Instance instance)
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
