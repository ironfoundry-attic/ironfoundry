namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json.Linq;

    /*
     * agent/lib/agent/message/*
     * methods:
     * get_state
     * prepare_network_change -> return true
     * compile_package
     * drain
     * get_task
     * stop -> is long_running?, Monit.stop_services, returns "stopped"
     * apply (HUGE)
     * start -> Monit.start_services, then returns "started"
     * ping -> returns "pong"
     * migrate_disk
     * list_disk
     * mount_disk
     * unmount_disk
     * noop -> returns "nope"
     */

    public abstract class BaseMessageHandler : IMessageHandler
    {
        public abstract HandlerResponse Handle(JObject parsed);

        public virtual void OnPostReply() { }
    }
}