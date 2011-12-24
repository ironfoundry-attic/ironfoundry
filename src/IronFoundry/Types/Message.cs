namespace IronFoundry.Types
{
    using Newtonsoft.Json;

    public abstract class Message : EntityBase
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
    }
}