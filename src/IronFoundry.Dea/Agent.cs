namespace IronFoundry.Dea
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Types;
    using NLog;
    using Providers;

    public sealed class Agent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan HEARTBEAT_INTERVAL = TimeSpan.FromSeconds(10);

        private readonly IMessagingProvider NATS;
        private readonly IWebServerAdministrationProvider IIS = new WebServerAdministrationProvider();
        private readonly DropletManager dropletManager = new DropletManager();
        private readonly FilesManager filesManager = new FilesManager();
        private readonly Hello helloMessage;
        private readonly Task monitorTask;

        private bool shutting_down = false;

        public Agent()
        {
            NATS = new NatsMessagingProvider(DeaConfig.NatsHost, DeaConfig.NatsPort);
            helloMessage = new Hello(NATS.UniqueIdentifier, Utility.LocalIPAddress, 12345, 0.99M);
            monitorTask = new Task(monitorLoop);
        }

        public bool Error { get; private set; }

        public bool Start()
        {
            bool rv = false;

            if (NATS.Connect())
            {
                NATS.Start();

                var vcapComponentDiscoverMessage = new VcapComponentDiscover(
                    argType: "DEA",
                    argIndex: 1,
                    argUuid: NATS.UniqueIdentifier, 
                    argHost: Utility.LocalIPAddress.ToString(),
                    argCredentials: NATS.UniqueIdentifier,
                    argStart: DateTime.Now);

                NATS.Subscribe(NatsSubscription.VcapComponentDiscover,
                    (msg, reply) =>
                    {
                        // TODO update_discover_uptime
                        NATS.Publish(reply, vcapComponentDiscoverMessage);
                    });

                NATS.Publish(new VcapComponentAnnounce(vcapComponentDiscoverMessage));

                NATS.Subscribe(NatsSubscription.DeaStatus, processDeaStatus);
                NATS.Subscribe(NatsSubscription.DropletStatus, processDropletStatus);
                NATS.Subscribe(NatsSubscription.DeaDiscover, processDeaDiscover);
                NATS.Subscribe(NatsSubscription.DeaFindDroplet, processDeaFindDroplet);
                NATS.Subscribe(NatsSubscription.DeaUpdate, processDeaUpdate);
                NATS.Subscribe(NatsSubscription.DeaStop, processDeaStop);
                NATS.Subscribe(NatsSubscription.GetDeaInstanceStartFor(NATS.UniqueIdentifier), processDeaStart);
                NATS.Subscribe(NatsSubscription.RouterStart, processRouterStart);
                NATS.Subscribe(NatsSubscription.HealthManagerStart, processHealthManagerStart);

                NATS.Publish(helloMessage);

                recoverExistingDroplets();

                monitorTask.Start();

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

            dropletManager.ForAllInstances(
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

            Logger.Debug(IronFoundry.Dea.Properties.Resources.Agent_WaitingForTasksInStop_Message);

            monitorTask.Wait(TimeSpan.FromSeconds(30));

            Logger.Debug(IronFoundry.Dea.Properties.Resources.Agent_TasksCompletedInStop_Message);
        }

        private void monitorLoop()
        {
            while (false == shutting_down)
            {
                if (NatsMessagingStatus.RUNNING != NATS.Status)
                {
                    Logger.Error(IronFoundry.Dea.Properties.Resources.Agent_ErrorDetectedInNats_Message);
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
            if (false == droplet.FrameworkSupported)
            {
                Logger.Debug("This DEA does not support non-aspdotnet frameworks");
                return;
            }

            Instance instance = new Instance(droplet);

            if (filesManager.Stage(droplet, instance))
            {
                WebServerAdministrationBinding binding = IIS.InstallWebApp(filesManager.GetApplicationPathFor(instance), instance.IIsName);
                instance.Host = binding.Host;
                instance.Port = binding.Port;

                filesManager.BindServices(droplet, instance.IIsName);

                instance.StateTimestamp = Utility.GetEpochTimestamp();

                instance.OnDeaStart();

                if (false == shutting_down)
                {
                    sendSingleHeartbeat(new Heartbeat(instance));

                    registerWithRouter(instance, instance.Uris);

                    dropletManager.Add(droplet.ID, instance);

                    takeSnapshot();
                }
            }
        }

        private void processDeaUpdate(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting processDeaUpdate: {0}", message);

            Droplet droplet = Message.FromJson<Droplet>(message);

            dropletManager.ForAllInstances(droplet.ID, (argInstance) =>
                {
                    string[] currentUris = argInstance.Uris.ToArrayOrNull(); // NB: will create new array

                    argInstance.Uris = droplet.Uris;

                    var toRemove = currentUris.Except(droplet.Uris);

                    var toAdd = droplet.Uris.Except(currentUris);

                    unregisterWithRouter(argInstance, toRemove.ToArray());

                    registerWithRouter(argInstance, toAdd.ToArray());
                });

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

        private void processDeaStop(string message, string reply)
        {
            if (shutting_down)
                return;

            Logger.Debug("Starting processDeaStop: {0}", message);

            StopDroplet stopDropletMsg = Message.FromJson<StopDroplet>(message);

            uint dropletID = stopDropletMsg.DropletID;

            dropletManager.ForAllInstances(dropletID, (argInstance) =>
                {
                    if (stopDropletMsg.AppliesTo(argInstance))
                    {
                        argInstance.OnDeaStop();

                        // stop_droplet
                        if (false == argInstance.StopProcessed)
                        {
                            sendExitedMessage(argInstance);

                            if (argInstance.IsStartingOrRunning)
                            {
                                IIS.UninstallWebApp(argInstance.IIsName);
                            }

                            argInstance.DeaStopComplete();

                            filesManager.CleanupInstanceDirectory(argInstance);

                            // remove_instance_resources

                            dropletManager.InstanceStopped(dropletID, argInstance);

                            Logger.Info("Stopped instance {0}", argInstance.LogID);

                            takeSnapshot();
                        }
                    }
                });
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

            dropletManager.ForAllInstances((instance) =>
            {
                if (instance.DropletID == findDroplet.DropletID)
                {
                    if (instance.Version == findDroplet.Version)
                    {
                        if (findDroplet.States.Contains(instance.State))
                        {
                            var startDate = DateTime.ParseExact(instance.Start, Constants.JsonDateFormat, CultureInfo.InvariantCulture);
                            var span = DateTime.Now - startDate;
                            var response = new FindDropletResponse(NATS.UniqueIdentifier, instance, span);

                            if (response.State != VcapStates.RUNNING)
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

            dropletManager.ForAllInstances((instance) =>
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

            dropletManager.ForAllInstances((instance) =>
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
            if (shutting_down || dropletManager.IsEmpty)
                return;

            var heartbeats = new List<Heartbeat>();

            dropletManager.ForAllInstances((instance) =>
            {
                instance.UpdateState(getApplicationState(instance.IIsName));
                instance.StateTimestamp = Utility.GetEpochTimestamp();
                heartbeats.Add(new Heartbeat(instance));
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
                return VcapStates.DELETED;

            ApplicationInstanceStatus status = IIS.GetStatus(name);

            string rv;

            switch (status)
            {
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

        private void takeSnapshot()
        {
            Snapshot snapshot = dropletManager.GetSnapshot();
            filesManager.TakeSnapshot(snapshot);
        }

        private void recoverExistingDroplets()
        {
            Snapshot snapshot = filesManager.GetSnapshot();
            if (null != snapshot)
            {
                dropletManager.FromSnapshot(snapshot);
                sendHeartbeat();
                takeSnapshot();
            }
        }

        private void sendSingleHeartbeat(Heartbeat argHeartbeat)
        {
            var message = new DropletHeartbeat
            {
                Droplets = new[] { argHeartbeat }
            };
            NATS.Publish(message);
        }
    }
}