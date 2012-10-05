namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using IronFoundry.Bosh.Types;
    using Newtonsoft.Json.Linq;

    public class Apply : BaseMessageHandler
    {
        public Apply(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            var newSpec = parsed["arguments"].First;
            Spec newSpecObj = newSpec.ToObject<Spec>();
            config.SetState(newSpecObj);

            return new HandlerResponse(newSpec);
            /*
                logger.info("Applying: #{@new_spec.inspect}")

                if !@old_plan.deployment.empty? &&
                    @old_plan.deployment != @new_plan.deployment
                  raise Bosh::Agent::MessageHandlerError,
                        "attempt to apply #{@new_plan.deployment} " +
                        "to #{old_plan.deployment}"
                end

                # FIXME: tests
                # if @state["configuration_hash"] == @new_spec["configuration_hash"]
                #   return @state
                # end

                if @new_plan.configured?
                  begin
                    delete_job_monit_files
                    apply_job
                    apply_packages
                    configure_job
                    reload_monit
                    @platform.update_logging(@new_spec)
                  rescue Exception => e
                    raise Bosh::Agent::MessageHandlerError,
                          "#{e.message}: #{e.backtrace}"
                  end
                end

                # FIXME: assumption right now: if apply succeeds state should be
                # identical with apply spec
                Bosh::Agent::Config.state.write(@new_spec)
                @new_spec

              rescue Bosh::Agent::StateError => e
                raise Bosh::Agent::MessageHandlerError, e
             */
        }
    }
}