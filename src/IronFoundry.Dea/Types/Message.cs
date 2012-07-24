namespace IronFoundry.Dea.Types
{
    using IronFoundry.Nats.Client;
    using Newtonsoft.Json;

    public abstract class Message : EntityBase, INatsMessage
    {
        public const string RECEIVE_ONLY = "RECEIVE_ONLY";
        public const string REPLY_OK = "REPLY_OK";

        [JsonIgnore]
        public virtual string PublishSubject
        {
            get { return RECEIVE_ONLY; }
        }

        [JsonIgnore]
        public string RawJson { get; set; }

        public virtual bool CanPublishWithSubject(string subject)
        {
            return false;
        }
    }
}