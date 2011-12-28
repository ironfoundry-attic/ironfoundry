namespace IronFoundry.Dea.Types
{
    using System;
    using System.Globalization;
    using IronFoundry.Dea.Config;
    using Newtonsoft.Json;

    public class VcapComponentDiscover : VcapComponentBase
    {
        private const string publishSubject = "vcap.component.discover";

        public VcapComponentDiscover(string type, Guid uuid, string host, ServiceCredential credentials)
            : base(type, null, uuid, host, credentials, DateTime.Now) { }

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return publishSubject; }
        }

        [JsonProperty(PropertyName = "uptime")]
        public string Uptime { get; private set; }

        public override bool CanPublishWithSubject(string subject)
        {
            return false == subject.IsNullOrWhiteSpace();
        }

        public void UpdateUptime()
        {
            TimeSpan uptimeSpan = DateTime.Now - base.Start;
            this.Uptime = String.Format(CultureInfo.InvariantCulture, "{0}d:{1}h:{2}m:{3}s", uptimeSpan.Days, uptimeSpan.Hours, uptimeSpan.Minutes, uptimeSpan.Seconds);
        }
    }
}