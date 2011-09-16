namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public class DropletHeartbeat : Message
    {
        private const string publishSubject = "dea.heartbeat";

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return publishSubject; }
        }

        [JsonProperty(PropertyName = "droplets")]
        public Heartbeat[] Droplets { get; set; }
    }
}
