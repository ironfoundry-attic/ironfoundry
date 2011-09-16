namespace CloudFoundry.Net.Types
{
    using System;
    using Newtonsoft.Json;

    public class VcapComponentAnnounce : VcapComponentBase
    {
        private const string publishSubject = "vcap.component.announce";

        public VcapComponentAnnounce(
            string argType, int argIndex, Guid argUuid,
            string argHost, Guid argCredentials, DateTime argStart)
            : base(argType, argIndex, argUuid,
                   argHost, argCredentials, argStart) { }

        public VcapComponentAnnounce(VcapComponentBase argVcapComponentBase)
            : base(argVcapComponentBase) { }

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return publishSubject; }
        }
    }
}