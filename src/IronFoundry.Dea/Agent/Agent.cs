namespace IronFoundry.Dea.Agent
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Types;
    using Providers;

    public sealed class Agent : IAgent
    {
        private static readonly TimeSpan HEARTBEAT_INTERVAL = TimeSpan.FromSeconds(10);

        private readonly ILog log;
        private readonly IConfig config;
        private readonly IMessagingProvider messagingProvider;
        private readonly IWebServerAdministrationProvider webServerProvider;
        private readonly IDropletManager dropletManager;
        private readonly IFilesManager filesManager;

        private readonly Hello helloMessage;
        private readonly Task monitorTask;

        private bool shutting_down = false;

        public Agent(ILog log, IConfig config,
            IMessagingProvider messagingProvider,
            IFilesManager filesManager,
            IDropletManager dropletManager,
            IWebServerAdministrationProvider webServerAdministrationProvider)
        {
            this.log               = log;
            this.config            = config;
            this.messagingProvider = messagingProvider;
            this.filesManager      = filesManager;
            this.dropletManager    = dropletManager;
            this.webServerProvider = webServerAdministrationProvider;

            helloMessage = new Hello(messagingProvider.UniqueIdentifier, config.LocalIPAddress, config.FilesServicePort, 0.99M);
            monitorTask = new Task(monitorLoop);
        }

        public bool Error { get; private set; }

        public bool Start()
        {
            bool rv = false;

            if (messagingProvider.Connect())
            {
                messagingProvider.Start();

                var vcapComponentDiscoverMessage = new VcapComponentDiscover(
                    argType: "DEA",
                    argIndex: 1,
                    argUuid: messagingProvider.UniqueIdentifier, 
                    argHost: config.LocalIPAddress.ToString(),
                    argCredentials: messagingProvider.UniqueIdentifier,
                    argStart: DateTime.Now);

                messagingProvider.Subscribe(NatsSubscription.VcapComponentDiscover,
                    (msg, reply) =>
                    {
                        // TODO update_discover_uptime
                        messagingProvider.Publish(reply, vcapComponentDiscoverMessage);
                    });

                messagingProvider.Publish(new VcapComponentAnnounce(vcapComponentDiscoverMessage));

                messagingProvider.Subscribe(NatsSubscription.DeaStatus, processDeaStatus);
                messagingProvider.Subscribe(NatsSubscription.DropletStatus, processDropletStatus);
                messagingProvider.Subscribe(NatsSubscription.DeaDiscover, processDeaDiscover);
                messagingProvider.Subscribe(NatsSubscription.DeaFindDroplet, processDeaFindDroplet);
                messagingProvider.Subscribe(NatsSubscription.DeaUpdate, processDeaUpdate);
                messagingProvider.Subscribe(NatsSubscription.DeaStop, processDeaStop);
                messagingProvider.Subscribe(NatsSubscription.GetDeaInstanceStartFor(messagingProvider.UniqueIdentifier), processDeaStart);
                messagingProvider.Subscribe(NatsSubscription.RouterStart, processRouterStart);
                messagingProvider.Subscribe(NatsSubscription.HealthManagerStart, processHealthManagerStart);

                messagingProvider.Publish(helloMessage);

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

            log.Info("Evacuating applications..");

            dropletManager.ForAllInstances(
                (dropletID) =>
                {
                    log.Debug("Evacuating app {0}", dropletID);
                },
                (instance) =>
                {
                    if (instance.IsCrashed)
                        return;

                    instance.DeaEvacuation();

                    sendExitedNotification(instance);
                });

            takeSnapshot();

            messagingProvider.Dispose();

            log.Debug(IronFoundry.Dea.Properties.Resources.Agent_WaitingForTasksInStop_Message);

            monitorTask.Wait(TimeSpan.FromSeconds(30));

            log.Debug(IronFoundry.Dea.Properties.Resources.Agent_TasksCompletedInStop_Message);
        }

        private void monitorLoop()
        {
            while (false == shutting_down)
            {
                if (NatsMessagingStatus.RUNNING != messagingProvider.Status)
                {
                    log.Error(IronFoundry.Dea.Properties.Resources.Agent_ErrorDetectedInNats_Message);
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

            log.Debug("Starting processDeaStart: {0}", message);

            Droplet droplet = Message.FromJson<Droplet>(message);
            if (false == droplet.FrameworkSupported)
            {
                log.Debug("This DEA does not support non-aspdotnet frameworks");
                return;
            }

            var instance = new Instance(droplet);

            if (filesManager.Stage(droplet, instance))
            {
                WebServerAdministrationBinding binding = webServerProvider.InstallWebApp(filesManager.GetApplicationPathFor(instance), instance.IIsName);
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

            log.Debug("Starting processDeaUpdate: {0}", message);

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

            log.Debug("Starting processDeaDiscover: {0}", message);
            Discover discover = Message.FromJson<Discover>(message);
            messagingProvider.Publish(reply, helloMessage);
        }

        private void processDeaStop(string message, string reply)
        {
            if (shutting_down)
                return;

            log.Debug("Starting processDeaStop: {0}", message);

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
                                webServerProvider.UninstallWebApp(argInstance.IIsName);
                            }

                            argInstance.DeaStopComplete();

                            filesManager.CleanupInstanceDirectory(argInstance);

                            // remove_instance_resources

                            dropletManager.InstanceStopped(dropletID, argInstance);

                            log.Info("Stopped instance {0}", argInstance.LogID);

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

            log.Debug("Starting processDeaStatus: {0}", message);
            var statusMessage = new Status(helloMessage)
            {
                MaxMemory      = 4096,
                UsedMemory     = 0,
                ReservedMemory = 0,
                NumClients     = 20
            };
            messagingProvider.Publish(reply, statusMessage);
        }

        private void processDeaFindDroplet(string message, string reply)
        {
            if (shutting_down)
                return;

            log.Debug("Starting processDeaFindDroplet: {0}", message);

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

                            var response = new FindDropletResponse(messagingProvider.UniqueIdentifier, instance, span)
                            {
                                FileUri = config.FilesServiceUri.AbsoluteUri,
                                Credentials = config.FilesCredentials.ToArray(),
                            };

                            if (response.State != VcapStates.RUNNING)
                            {
                                response.Stats = null;
                            }

                            messagingProvider.Publish(reply, response);
                        }
                    }
                }
            });
        }

        private void processDropletStatus(string message, string reply)
        {
            if (shutting_down)
                return;

            log.Debug("Starting processDropletStatus: {0}", message);

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
                    messagingProvider.Publish(reply, response);
                }
            });
        }

        private void processRouterStart(string message, string reply)
        {
            if (shutting_down)
                return;

            log.Debug("Starting processRouterStart: {0}", message);

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
            log.Debug("Starting processHealthManagerStart: {0}", message);
            sendHeartbeat();
        }

        private void registerWithRouter(Instance argInstance, string[] argUris)
        {
            if (shutting_down || argInstance.Uris.IsNullOrEmpty())
                return;

            var routerRegister = new RouterRegister
            {
                Dea  = messagingProvider.UniqueIdentifier,
                Host = argInstance.Host,
                Port = argInstance.Port,
                Uris = argUris ?? argInstance.Uris,
                Tag  = new Tag { Framework = argInstance.Framework, Runtime = argInstance.Runtime }
            };

            messagingProvider.Publish(routerRegister);
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
                Dea  = messagingProvider.UniqueIdentifier,
                Host = argInstance.Host,
                Port = argInstance.Port,
                Uris = argUris ?? argInstance.Uris,
            };

            messagingProvider.Publish(routerUnregister);
        }

        private void sendExitedNotification(Instance argInstance)
        {
            if (argInstance.IsEvacuated)
                return;

            messagingProvider.Publish(new InstanceExited(argInstance));

            log.Debug("Sent droplet.exited.");
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

            messagingProvider.Publish( message);
        }

        private string getApplicationState(string name)
        {
            if (false == webServerProvider.DoesApplicationExist(name))
                return VcapStates.DELETED;

            ApplicationInstanceStatus status = webServerProvider.GetStatus(name);

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
            messagingProvider.Publish(message);
        }
    }
}