namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class GetTask : BaseMessageHandler
    {
        public GetTask(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            /*
             * agent/lib/agent/handler.rb
             * 
    def handle_get_task(reply_to, agent_task_id)
      if @long_running_agent_task == [agent_task_id]
        publish(reply_to, {"value" => {"state" => "running", "agent_task_id" => agent_task_id}})
      else
        rs = @results.find { |time, task_id, result| task_id == agent_task_id }
        if rs
          time, task_id, result = rs
          publish(reply_to, result)
        else
          publish(reply_to, {"exception" => "unknown agent_task_id" })
        end
      end
    end
             */

            /*
             * TODO - none of the handlers send back messages indicating that they are long-running,
             * so this handler probably won't get called.
             */
            var firstArg = parsed["arguments"].First;
            return new HandlerResponse(new { state = "completed" });
        }
    }
}