namespace IronFoundry.Bosh.Agent.Handlers
{
    using System;
    using IronFoundry.Bosh.Configuration;
    using IronFoundry.Bosh.Properties;
    using IronFoundry.Nats.Client;
    using Newtonsoft.Json.Linq;

    public class Drain : BaseMessageHandler
    {
        private readonly INatsClient nats;

        public Drain(IBoshConfig config, INatsClient nats)
            : base(config)
        {
            this.nats = nats;
        }

        // agent/lib/agent/message/drain.rb
        // long_running? -> true
        public override HandlerResponse Handle(JObject parsed)
        {
            var args = parsed["arguments"];
            string drainType  = (string)args[0]; // shutdown / update / status

            object value = null;

            switch (drainType)
            {
                case "shutdown" :
                    value = DrainForShutdown();
                    break;
                case "update" :
                    value = DrainForUpdate();
                    break;
                case "status" :
                    value = DrainCheckStatus();
                    break;
                default:
                    throw new MessageHandlerException(String.Format(Resources.Drain_UnknownDrainType_Fmt, drainType));
            }

            return new HandlerResponse(value);
        }

        private object DrainForShutdown()
        {
            nats.Publish(String.Format("hm.agent.shutdown.{0}", config.AgentID));
            return 0;
        }

        private object DrainForUpdate()
        {
            /*
        if @spec.nil?
          raise Bosh::Agent::MessageHandlerError, "Drain update called without apply spec"
        end
        if @old_spec.key?('job') && drain_script_exists?
          # HACK: We go through the motions below to be able to support drain scripts written as shell scripts
          run_drain_script(job_change, hash_change, updated_packages.flatten)
        else
          0
        end
             */
            return 0;
        }

        private object DrainCheckStatus()
        {
            return 0;
        }
    }
}