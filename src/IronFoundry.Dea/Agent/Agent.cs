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
    using IronFoundry.Dea.Properties;
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
            monitorTask = new Task(MonitorLoop);
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

                messagingProvider.Subscribe(NatsSubscription.DeaStatus, ProcessDeaStatus);
                messagingProvider.Subscribe(NatsSubscription.DropletStatus, ProcessDropletStatus);
                messagingProvider.Subscribe(NatsSubscription.DeaDiscover, ProcessDeaDiscover);
                messagingProvider.Subscribe(NatsSubscription.DeaFindDroplet, ProcessDeaFindDroplet);
                messagingProvider.Subscribe(NatsSubscription.DeaUpdate, ProcessDeaUpdate);
                messagingProvider.Subscribe(NatsSubscription.DeaStop, ProcessDeaStop);
                messagingProvider.Subscribe(NatsSubscription.GetDeaInstanceStartFor(messagingProvider.UniqueIdentifier), ProcessDeaStart);
                messagingProvider.Subscribe(NatsSubscription.RouterStart, ProcessRouterStart);
                messagingProvider.Subscribe(NatsSubscription.HealthManagerStart, ProcessHealthManagerStart);

                messagingProvider.Publish(helloMessage);

                RecoverExistingDroplets();

                monitorTask.Start();

                rv = true;
            }

            return rv;
        }

        public void Stop()
        {
            if (shutting_down)
            {
                return;
            }

            shutting_down = true;

            log.Info("Evacuating applications..");

            dropletManager.ForAllInstances(
                (dropletID) =>
                {
                    log.Debug("Evacuating app {0}", dropletID);
                },
                (instance) =>
                {
                    if (false == instance.IsCrashed)
                    {
                        instance.DeaEvacuation();
                        SendExitedNotification(instance);
                        instance.Evacuated();
                    }
                });

            TakeSnapshot();

            log.Debug(Resources.Agent_WaitingForTasksInStop_Message);
            monitorTask.Wait(TimeSpan.FromSeconds(30));
            log.Debug(Resources.Agent_TasksCompletedInStop_Message);

            Shutdown();
        }

        private void Shutdown()
        {
            shutting_down = true;
            log.Info("Shutting down..");
            dropletManager.ForAllInstances((instance) =>
                {
                    instance.DeaShutdown();
                    StopDroplet(instance);
                });
            TakeSnapshot();
            messagingProvider.Dispose();
            log.Info("Shutting down complete.");
        }

        private void MonitorLoop()
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
                    SendHeartbeat();
                }
                Thread.Sleep(HEARTBEAT_INTERVAL);
            }
        }

        private void ProcessDeaStart(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

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
                if (null == binding)
                {
                    log.Error(Resources.Agent_ProcessDeaStartNoBindingAvailable, instance.IIsName);
                    filesManager.CleanupInstanceDirectory(instance, true);
                }
                else
                {
                    instance.Host = binding.Host;
                    instance.Port = binding.Port;

                    filesManager.BindServices(droplet, instance.IIsName);

                    instance.StateTimestamp = Utility.GetEpochTimestamp();

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

            log.Debug("Starting processDeaUpdate: {0}", message);

            Droplet droplet = Message.FromJson<Droplet>(message);

            dropletManager.ForAllInstances(droplet.ID, (argInstance) =>
                {
                    string[] currentUris = argInstance.Uris.ToArrayOrNull(); // NB: will create new array

                    argInstance.Uris = droplet.Uris;

                    var toRemove = currentUris.Except(droplet.Uris);

                    var toAdd = droplet.Uris.Except(currentUris);

                    UnregisterWithRouter(argInstance, toRemove.ToArray());

                    RegisterWithRouter(argInstance, toAdd.ToArray());
                });

            TakeSnapshot();
        }

        private void ProcessDeaDiscover(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

            log.Debug("Starting processDeaDiscover: {0}", message);
            Discover discover = Message.FromJson<Discover>(message);
            messagingProvider.Publish(reply, helloMessage);
        }

        private void ProcessDeaStop(string message, string reply)
        {
            if (shutting_down)
                return;

            log.Debug("Starting processDeaStop: {0}", message);

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

            log.Info("Stopping instance {0}", instance.LogID);

            SendExitedMessage(instance);

            webServerProvider.UninstallWebApp(instance.IIsName);

            dropletManager.InstanceStopped(instance);

            instance.DeaStopComplete();

            filesManager.CleanupInstanceDirectory(instance);

            log.Info("Stopped instance {0}", instance.LogID);
        }

        private void SendExitedMessage(Instance argInstance)
        {
            if (argInstance.IsNotified)
            {
                return;
            }

            UnregisterWithRouter(argInstance);

            if (false == argInstance.HasExitReason)
            {
                argInstance.Crashed();
            }

            SendExitedNotification(argInstance);

            argInstance.IsNotified = true;
        }

        private void ProcessDeaStatus(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

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

        private void ProcessDeaFindDroplet(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

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

        private void ProcessDropletStatus(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

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

        private void ProcessRouterStart(string message, string reply)
        {
            if (shutting_down)
            {
                return;
            }

            log.Debug("Starting processRouterStart: {0}", message);

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
            log.Debug("Starting processHealthManagerStart: {0}", message);
            SendHeartbeat();
        }

        private void RegisterWithRouter(Instance argInstance, string[] argUris)
        {
            if (shutting_down || argInstance.Uris.IsNullOrEmpty())
            {
                return;
            }

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

        private void UnregisterWithRouter(Instance argInstance)
        {
            UnregisterWithRouter(argInstance, null);
        }

        private void UnregisterWithRouter(Instance argInstance, string[] argUris)
        {
            if (shutting_down || argInstance.Uris.IsNullOrEmpty())
            {
                return;
            }

            var routerUnregister = new RouterUnregister
            {
                Dea  = messagingProvider.UniqueIdentifier,
                Host = argInstance.Host,
                Port = argInstance.Port,
                Uris = argUris ?? argInstance.Uris,
            };

            messagingProvider.Publish(routerUnregister);
        }

        private void SendExitedNotification(Instance argInstance)
        {
            if (false == argInstance.IsEvacuated)
            {
                messagingProvider.Publish(new InstanceExited(argInstance));
                log.Debug("Sent droplet.exited.");
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
                instance.UpdateState(GetApplicationState(instance.IIsName));
                instance.StateTimestamp = Utility.GetEpochTimestamp();
                heartbeats.Add(new Heartbeat(instance));
            });

            var message = new DropletHeartbeat
            {
                Droplets = heartbeats.ToArray()
            };

            messagingProvider.Publish( message);
        }

        private string GetApplicationState(string name)
        {
            if (false == webServerProvider.DoesApplicationExist(name))
            {
                return VcapStates.DELETED;
            }

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

        private void SendSingleHeartbeat(Heartbeat argHeartbeat)
        {
            var message = new DropletHeartbeat
            {
                Droplets = new[] { argHeartbeat }
            };
            messagingProvider.Publish(message);
        }
    }
}