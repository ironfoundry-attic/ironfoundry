namespace IronFoundry.Dea.Types
{
    using System;
    using Newtonsoft.Json;

    public class VcapComponentDiscover : VcapComponentBase
    {
        private const string publishSubject = "vcap.component.discover";

        public VcapComponentDiscover(
            string argType, int argIndex, Guid argUuid,
            string argHost, Guid argCredentials, DateTime argStart)
            : base(argType, argIndex, argUuid,
                   argHost, argCredentials, argStart) { }

        public VcapComponentDiscover(VcapComponentBase argVcapComponentBase)
            : base(argVcapComponentBase) { }

        [JsonIgnore]
        public override string PublishSubject
        {
            get { return publishSubject; }
        }
    }
}