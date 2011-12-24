namespace IronFoundry.Dea.Types
{
    using System;
    using Newtonsoft.Json;

    public class VcapComponentDiscover : VcapComponentBase
    {
        private const string publishSubject = "vcap.component.discover";

        public VcapComponentDiscover(
            string type, int index, Guid uuid,
            string host, Guid credentials, DateTime start)
            : base(type, index, uuid, host, credentials, start) { }

        public VcapComponentDiscover(VcapComponentBase vcapComponentBase)
            : base(vcapComponentBase) { }

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return publishSubject; }
        }

        public override bool CanPublishWithSubject(string subject)
        {
            return false == subject.IsNullOrWhiteSpace();
        }
    }
}
