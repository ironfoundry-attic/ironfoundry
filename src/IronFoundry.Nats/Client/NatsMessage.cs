namespace IronFoundry.Nats.Client
{
    using Newtonsoft.Json;

    public abstract class NatsMessage : INatsMessage
    {
        // TODO is this really useful?
        public virtual bool CanPublishWithSubject(string subject)
        {
            return true;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override string ToString()
        {
            return ToJson();
        }

        [JsonIgnore]
        public abstract string PublishSubject { get; }

        [JsonIgnore]
        public bool IsReceiveOnly
        {
            get { return false; }
        }
    }
}