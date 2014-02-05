using IronFoundry.Nats.Client;
using Newtonsoft.Json;

namespace IronFoundry.Dea.Types
{
    public abstract class Message : EntityBase, INatsMessage
    {
        public const string ReceiveOnly = "ReceiveOnly";
        public const string ReplyOk = "ReplyOk";

        [JsonIgnore]
        public string RawJson { get; set; }

        [JsonIgnore]
        public virtual string PublishSubject
        {
            get { return ReceiveOnly; }
        }

        [JsonIgnore]
        public bool IsReceiveOnly
        {
            get { return PublishSubject == ReceiveOnly; }
        }

        public virtual bool CanPublishWithSubject(string subject)
        {
            return false;
        }
    }
}