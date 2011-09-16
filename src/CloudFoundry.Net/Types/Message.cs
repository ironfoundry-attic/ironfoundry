namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public abstract class Message : JsonBase
    {
        public const string RECEIVE_ONLY = "RECEIVE_ONLY";
        public const string REPLY_OK = "REPLY_OK";

        [JsonIgnore]
        public virtual string PublishSubject
        {
            get { return RECEIVE_ONLY; }
        }
    }
}