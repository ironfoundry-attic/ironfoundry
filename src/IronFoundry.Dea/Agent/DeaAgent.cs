namespace IronFoundry.Dea.Agent
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Dea.Configuration;
    using IronFoundry.Dea.Properties;
    using IronFoundry.Dea.Types;
    using IronFoundry.Misc;
    using IronFoundry.Misc.Agent;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Nats.Client;
    using Providers;

    public sealed class DeaAgent : IAgent
    {
        private readonly TimeSpan OneSecondInterval = TimeSpan.FromSeconds(1);
        private readonly TimeSpan TwoSecondsInterval = TimeSpan.FromSeconds(2);
        private readonly TimeSpan FiveSecondsInterval = TimeSpan.FromSeconds(5);
        private readonly TimeSpan TenSecondsInterval = TimeSpan.FromSeconds(10);

        private readonly ILog log;
        private readonly IDeaConfig config;
        private readonly INatsClient natsClient;
        private readonly IFilesManager filesManager;
        private readonly IConfigManager configManager;
        private readonly IDropletManager dropletManager;
        private readonly IWebServerAdministrationProvider webServerProvider;
        private readonly IVarzProvider varzProvider;

        private readonly Hello helloMessage;

        private readonly Task processTask;
        private readonly Task heartbeatTask;
        private readonly Task advertiseTask;
        private readonly Task varzTask;
        private readonly Task monitorAppsTask;

        private bool shutting_down = false;

        private ushort maxMemoryMB;

        private VcapComponentDiscover discoverMessage;

        public DeaAgent(ILog log, IDeaConfig config,
            INatsClient natsClient,
            IFilesManager filesManager,
            IConfigManager configManager,
            IDropletManager dropletManager,
            IWebServerAdministrationProvider webServerAdministrationProvider,
            IVarzProvider varzProvider)
        {
            this.log               = log;
            this.config            = config;
            this.natsClient        = natsClient;
            this.filesManager      = filesManager;
            this.configManager     = configManager;
            this.dropletManager    = dropletManager;
            this.webServerProvider = webServerAdministrationProvider;
            this.varzProvider      = varzProvider;

            helloMessage = new Hello(natsClient.UniqueIdentifier, config.LocalIPAddress, config.FilesServicePort);

            processTask     = new Task(ProcessLoop);
            heartbeatTask   = new Task(HeartbeatLoop);
            advertiseTask   = new Task(AdvertiseLoop);
            varzTask        = new Task(SnapshotVarz);
            monitorAppsTask = new Task(MonitorLoop);

            this.maxMemoryMB = config.MaxMemoryMB;
        }

        public string Name { get { return "Dea"; } }

        public bool Error { get; private set; }

        public string[] ProgramArguments
        {
            get; set;
        }

        public void Start()
        {
            if (natsClient.Start())
            {
                discoverMessage = new VcapComponentDiscover(
                    type: Resources.Agent_DEAComponentType,
                    uuid: natsClient.UniqueIdentifier,
                    host: config.MonitoringServiceHostStr,
                    credentials: config.MonitoringCredentials);

                varzProvider.Discover = discoverMessage;

                natsClient.Subscribe(NatsSubscription.VcapComponentDiscover,
                    (msg, reply) =>
                    {
                        discoverMessage.UpdateUptime();
                        natsClient.Publish(reply, discoverMessage);
                    });

                natsClient.Publish(new VcapComponentAnnounce(discoverMessage));

                natsClient.Subscribe(NatsSubscription.DeaStatus, ProcessDeaStatus);
                natsClient.Subscribe(NatsSubscription.DropletStatus, ProcessDropletStatus);
                natsClient.Subscribe(NatsSubscription.DeaDiscover, ProcessDeaDiscover);
                natsClient.Subscribe(NatsSubscription.DeaFindDroplet, ProcessDeaFindDroplet);
                natsClient.Subscribe(NatsSubscription.DeaUpdate, ProcessDeaUpdate);
                natsClient.Subscribe(NatsSubscription.DeaStop, ProcessDeaStop);
                natsClient.Subscribe(NatsSubscription.GetDeaInstanceStartFor(natsClient.UniqueIdentifier), ProcessDeaStart);
                natsClient.Subscribe(NatsSubscription.RouterStart, ProcessRouterStart);
                natsClient.Subscribe(NatsSubscription.HealthManagerStart, ProcessHealthManagerStart);
                natsClient.Subscribe(NatsSubscription.DeaLocate, ProcessDeaLocate);

                natsClient.Publish(helloMessage);

                SendAdvertise();

                RecoverExistingDroplets();

                processTask.Start();
                heartbeatTask.Start();
                advertiseTask.Start();
                varzTask.Start();
                monitorAppsTask.Start();
            }
            else
            {
                log.Error(Resources.Agent_ErrorDetectedInNats_Message);
                Error = true;
            }
        }

        public void Stop()
        {
            if (shutting_down)
            {
                return;
            }

            shutting_down = true;

            log.Info(Resources.Agent_ShuttingDown_Message);

            dropletManager.ForAllInstances((instance) =>
                {
                    if (false == instance.IsCrashed)
                    {
                        instance.DeaShutdown();
                        StopDroplet(instance);
                    }
                });

            TakeSnapshot();

            natsClient.Dispose();

            log.Info(Resources.Agent_Shutdown_Message);
        }

        private void ProcessLoop()
        {
            while (false == shutting_down)
            {
                var iisWorkerProcesses = webServerProvider.GetIIsWorkerProcesses();
                dropletManager.SetProcessInformationFrom(iisWorkerProcesses);
                Thread.Sleep(FiveSecondsInterval);
            }
        }

        private void HeartbeatLoop()
        {
            while (false == shutting_down)
            {
                SendHeartbeat();
                Thread.Sleep(TenSecondsInterval);
            }
        }

        private void AdvertiseLoop()
        {
            while (false == shutting_down)
            {
                SendAdvertise();
                Thread.Sleep(FiveSecondsInterval);
            }
        }

        private void ProcessDeaStart(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

            log.Debug(Resources.Agent_ProcessDeaStart_Fmt, message);

            Droplet droplet = Message.FromJson<Droplet>(message);
            if (false == droplet.FrameworkSupported)
            {
                log.Info(Resources.Agent_NonAspDotNet_Message);
                return;
            }

            var instance = new Instance(config.AppDir, droplet);

            if (filesManager.Stage(droplet, instance))
            {
                WebServerAdministrationBinding binding = webServerProvider.InstallWebApp(
                    filesManager.GetApplicationPathFor(instance), instance.Staged, instance.ManagedRuntimeVersion);
                if (null == binding)
                {
                    log.Error(Resources.Agent_ProcessDeaStartNoBindingAvailable, instance.Staged);
                    filesManager.CleanupInstanceDirectory(instance, true);
                }
                else
                {
                    instance.Host = binding.Host;
                    instance.Port = binding.Port;

                    configManager.BindServices(droplet, instance);
                    configManager.SetupEnvironment(droplet, instance);

                    instance.OnDeaStart();

                    if (false == shutting_down)
                    {
                        SendSingleHeartbeat(new Heartbeat(instance));

                        RegisterWithRouter(instance, instance.Uris);

                        dropletManager.Add(droplet.ID, instance);

                        TakeSnapshot();
                    }
                }
            }
        }

        private void ProcessDeaUpdate(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

            log.Debug(Resources.Agent_ProcessDeaUpdate_Fmt, message);

            Droplet droplet = Message.FromJson<Droplet>(message);

            dropletManager.ForAllInstances(droplet.ID, (instance) =>
                {
                    string[] currentUris = instance.Uris.ToArrayOrNull(); // NB: will create new array

                    instance.Uris = droplet.Uris;

                    var toRemove = currentUris.Except(droplet.Uris);

                    var toAdd = droplet.Uris.Except(currentUris);

                    UnregisterWithRouter(instance, toRemove.ToArray());

                    RegisterWithRouter(instance, toAdd.ToArray());
                });

            TakeSnapshot();
        }

        private void ProcessDeaLocate(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }
            SendAdvertise();
        }

        private void ProcessDeaDiscover(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

            log.Debug(Resources.Agent_ProcessDeaDiscover_Fmt, message, reply);

            Discover discover = Message.FromJson<Discover>(message);
            if (Constants.IsSupportedRuntime(discover.Runtime))
            {
                uint delay = 0;
                dropletManager.ForAllInstances(discover.DropletID, (instance) =>
                    {
                        delay += 10; // NB: 10 milliseconds delay per app
                    });
                natsClient.PublishReply(reply, helloMessage, Math.Min(delay, 250));
            }
            else
            {
                log.Debug(Resources.Agent_ProcessDeaDiscoverUnsupportedRuntime_Fmt, discover.Runtime);
            }
        }

        private void ProcessDeaStop(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

            log.Debug(Resources.Agent_ProcessDeaStop_Fmt, message);

            StopDroplet stopDropletMsg = Message.FromJson<StopDroplet>(message);

            uint dropletID = stopDropletMsg.DropletID;

            dropletManager.ForAllInstances(dropletID, (instance) =>
                {
                    if (stopDropletMsg.AppliesTo(instance))
                    {
                        instance.OnDeaStop();
                        StopDroplet(instance);
                    }
                });
        }

        private void StopDroplet(Instance instance)
        {
            if (instance.StopProcessed)
            {
                return;
            }

            log.Info(Resources.Agent_StoppingInstance_Fmt, instance.LogID);

            SendExitedMessage(instance);

            webServerProvider.UninstallWebApp(instance.Staged);

            dropletManager.InstanceStopped(instance);

            instance.DeaStopComplete();

            filesManager.CleanupInstanceDirectory(instance);

            log.Info(Resources.Agent_StoppedInstance_Fmt, instance.LogID);
        }

        private void SendExitedMessage(Instance instance)
        {
            if (instance.IsNotified)
            {
                return;
            }

            UnregisterWithRouter(instance);

            if (false == instance.HasExitReason)
            {
                instance.Crashed();
            }

            SendExitedNotification(instance);

            instance.IsNotified = true;
        }

        private void ProcessDeaStatus(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

            log.Debug(Resources.Agent_ProcessDeaStatus_Fmt, message, reply);

            var statusMessage = new Status(helloMessage)
            {
                MaxMemory      = 4096,
                UsedMemory     = 0,
                ReservedMemory = 0,
                NumClients     = 20
            };
            natsClient.Publish(reply, statusMessage);
        }

        private void ProcessDeaFindDroplet(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

            log.Debug(Resources.Agent_ProcessDeaFindDroplet_Fmt, message);

            FindDroplet findDroplet = Message.FromJson<FindDroplet>(message);

            dropletManager.ForAllInstances(findDroplet.DropletID, (instance) =>
            {
                bool versionMatched = findDroplet.Version.IsNullOrWhiteSpace() || instance.Version == findDroplet.Version;
                bool instanceMatched = null == findDroplet.InstanceIds || findDroplet.InstanceIds.Contains(instance.InstanceID);
                bool indexMatched = null == findDroplet.Indices || findDroplet.Indices.Contains(instance.InstanceIndex);
                bool stateMatched = null == findDroplet.States || findDroplet.States.Contains(instance.State);

                if (versionMatched && instanceMatched && indexMatched && stateMatched)
                {
                    var response = new FindDropletResponse(natsClient.UniqueIdentifier, instance)
                    {
                        FileUri = String.Format(CultureInfo.InvariantCulture,
                            Resources.Agent_Droplets_Fmt, config.LocalIPAddress, config.FilesServicePort),
                        Credentials = config.FilesCredentials.ToArray(),
                    };

                    if (findDroplet.IncludeStats && instance.IsRunning)
                    {
                        response.Stats = new Stats(instance);
                    }

                    natsClient.Publish(reply, response);
                }
            });
        }

        private void ProcessDropletStatus(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

            log.Debug(Resources.Agent_ProcessDropletStatus_Fmt, message, reply);

            dropletManager.ForAllInstances((instance) =>
            {
                if (instance.CanGatherStats)
                {
                    var response = new Stats(instance); // TODO more statistics
                    natsClient.Publish(reply, response);
                }
            });
        }

        private void ProcessRouterStart(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

            log.Debug(Resources.Agent_ProcessRouterStart_Fmt, message, reply);

            dropletManager.ForAllInstances((instance) =>
                {
                    if (instance.IsRunning)
                    {
                        RegisterWithRouter(instance, instance.Uris);
                    }
                });
        }

        private void ProcessHealthManagerStart(string message, string reply)
        {
            log.Debug(Resources.Agent_ProcessHealthManagerStart_Fmt, message, reply);
            SendHeartbeat();
        }

        private void RegisterWithRouter(Instance instance, string[] uris)
        {
            if (shutting_down || instance.Uris.IsNullOrEmpty())
            {
                return;
            }

            var routerRegister = new RouterRegister
            {
                Dea  = natsClient.UniqueIdentifier,
                Host = instance.Host,
                Port = instance.Port,
                Uris = uris ?? instance.Uris,
                Tag  = new Tag { Framework = instance.Framework, Runtime = instance.Runtime }
            };

            natsClient.Publish(routerRegister);
        }

        private void UnregisterWithRouter(Instance instance)
        {
            UnregisterWithRouter(instance, instance.Uris);
        }

        private void UnregisterWithRouter(Instance instance, string[] uris)
        {
            if (instance.Uris.IsNullOrEmpty())
            {
                return;
            }

            var routerUnregister = new RouterUnregister
            {
                Dea  = natsClient.UniqueIdentifier,
                Host = instance.Host,
                Port = instance.Port,
                Uris = uris ?? instance.Uris,
            };

            natsClient.Publish(routerUnregister);
        }

        private void SendExitedNotification(Instance instance)
        {
            if (false == instance.IsEvacuated)
            {
                natsClient.Publish(new InstanceExited(instance));
                log.Debug(Resources.Agent_InstanceExited_Fmt, instance.LogID);
            }
        }

        private void SendHeartbeat()
        {
            if (shutting_down || dropletManager.IsEmpty)
            {
                return;
            }

            var heartbeats = new List<Heartbeat>();

            dropletManager.ForAllInstances((instance) =>
            {
                instance.UpdateState(GetApplicationState(instance.Staged));
                instance.StateTimestamp = Utility.GetEpochTimestamp();
                heartbeats.Add(new Heartbeat(instance));
            });

            var message = new DropletHeartbeat
            {
                Droplets = heartbeats.ToArray()
            };

            natsClient.Publish(message);
        }

        private void SendAdvertise()
        {
            if (shutting_down) // || no resources
            {
                return;
            }
            var message = new Advertise(natsClient.UniqueIdentifier, 4096, 0, true); // TODO mem
            natsClient.Publish(message);
        }

        private string GetApplicationState(string name)
        {
            ApplicationInstanceStatus status = webServerProvider.GetApplicationStatus(name);
            string rv;
            switch (status)
            {
                case ApplicationInstanceStatus.Deleted:
                    rv = VcapStates.DELETED;
                    break;
                case ApplicationInstanceStatus.Started:
                    rv = VcapStates.RUNNING;
                    break;
                case ApplicationInstanceStatus.Starting:
                    rv = VcapStates.STARTING;
                    break;
                case ApplicationInstanceStatus.Stopping:
                    rv = VcapStates.SHUTTING_DOWN;
                    break;
                case ApplicationInstanceStatus.Stopped:
                    rv = VcapStates.STOPPED;
                    break;
                case ApplicationInstanceStatus.Unknown:
                    rv = VcapStates.CRASHED;
                    break;
                default:
                    rv = VcapStates.CRASHED;
                    break;
            }
            return rv;
        }

        private void TakeSnapshot()
        {
            Snapshot snapshot = dropletManager.GetSnapshot();
            filesManager.TakeSnapshot(snapshot);
        }

        private void RecoverExistingDroplets()
        {
            Snapshot snapshot = filesManager.GetSnapshot();
            if (null != snapshot)
            {
                dropletManager.FromSnapshot(snapshot);
                SendHeartbeat();
                TakeSnapshot();
            }
        }

        private void SendSingleHeartbeat(Heartbeat heartbeat)
        {
            var message = new DropletHeartbeat
            {
                Droplets = new[] { heartbeat }
            };
            natsClient.Publish(message);
        }

        private void SnapshotVarz()
        {
            while (false == shutting_down)
            {
                varzProvider.MaxMemoryMB = this.maxMemoryMB;
                varzProvider.MemoryReservedMB = 0; // TODO
                varzProvider.MemoryUsedMB = 0; // TODO
                varzProvider.MaxClients = 1024;
                if (shutting_down)
                {
                    varzProvider.State = VcapStates.SHUTTING_DOWN;
                }
                Thread.Sleep(OneSecondInterval);
            }
        }

        private void MonitorLoop()
        {
            while (false == shutting_down)
            {
                Thread.Sleep(TwoSecondsInterval);

                var runningAppsJson = new List<string>();

                if (dropletManager.IsEmpty)
                {
                    varzProvider.MemoryUsedMB = 0;
                    continue;
                }

                var metrics = new Dictionary<string, IDictionary<string, Metric>>
                {
                    { "framework", new Dictionary<string, Metric>() }, 
                    { "runtime", new Dictionary<string, Metric>() }
                };

                DateTime monitorPassStart = DateTime.Now;

                dropletManager.ForAllInstances((instance) =>
                    {
                        if (false == instance.IsRunning)
                        {
                            return;
                        }

                        instance.CalculateUsage();

                        foreach (KeyValuePair<string, IDictionary<string, Metric>> kvp in metrics)
                        {
                            var metric = new Metric();

                            if (kvp.Key == "framework")
                            {
                                if (false == metrics.ContainsKey(instance.Framework))
                                {
                                    kvp.Value[instance.Framework] = metric;
                                }
                                metric = kvp.Value[instance.Framework];
                            }

                            if (kvp.Key == "runtime")
                            {
                                if (false == metrics.ContainsKey(instance.Runtime))
                                {
                                    kvp.Value[instance.Runtime] = metric;
                                }
                                metric = kvp.Value[instance.Runtime];
                            }

                            metric.UsedMemory += 0; // TODO KB
                            metric.ReservedMemory += 0; // TODO KB
                            metric.UsedDisk += 0; // TODO BYTES
                            metric.UsedCpu += 0; // TODO
                        }

                        string instanceJson = instance.ToJson();
                        runningAppsJson.Add(instanceJson);
                    }
                );

                varzProvider.RunningAppsJson = runningAppsJson;
                varzProvider.FrameworkMetrics = metrics["framework"];
                varzProvider.RuntimeMetrics = metrics["runtime"];
            }
        }
    }
}