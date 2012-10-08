namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class ListDisk : BaseMessageHandler
    {
        public ListDisk(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            // agent/lib/agent/message/disk.rb
            return new HandlerResponse("TODO");
        }
    }
}