namespace IronFoundry.Nats.Client
{
    using Newtonsoft.Json;

    public class Connect : NatsMessage
    {
        [JsonProperty(PropertyName = "verbose")]
        public bool Verbose { get; set; }

        [JsonProperty(PropertyName = "pedantic")]
        public bool Pedantic { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        [JsonProperty(PropertyName = "pass")]
        public string Password { get; set; }

        /// <summary>
        /// TODO 201207
        /// </summary>
        public override string PublishSubject
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}