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

        public static class Subjects
        {
            public const string DeaInstanceStart = "dea.{0:N}.start"; // NB: argument is GUID
            public const string DeaStop = "dea.stop";
            public const string DeaStatus = "dea.status";
            public const string DropletStatus = "droplet.status";
            public const string DeaDiscover = "dea.discover";
            public const string DeaFindDroplet = "dea.find.droplet";
            public const string DeaUpdate = "dea.update";
            public const string RouterStart = "router.start";
            public const string HealthManagerStart = "healthmanager.start";
        }
    }
}