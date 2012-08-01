namespace IronFoundry.Dea.Types
{
    using System;
    using IronFoundry.Dea.Configuration;
    using Newtonsoft.Json;

    public class VcapComponentAnnounce : VcapComponentBase
    {
        private const string publishSubject = "vcap.component.announce";

        public VcapComponentAnnounce(string type, string index, Guid uuid, string host, ServiceCredential credentials, DateTime start)
            : base(type, index, uuid, host, credentials, start) { }

        public VcapComponentAnnounce(VcapComponentBase vcapComponentBase) : base(vcapComponentBase) { }

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return publishSubject; }
        }
    }
}