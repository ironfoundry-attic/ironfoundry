namespace CloudFoundry.Net.Types
{
    using System;
    using JsonConverters;
    using Newtonsoft.Json;

    public abstract class VcapComponentBase : Message
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; private set; }

        [JsonProperty(PropertyName = "index")]
        public int Index { get; private set; }

        [JsonProperty(PropertyName = "uuid"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid Uuid { get; private set; }

        [JsonProperty(PropertyName = "host")]
        public string Host { get; private set; }

        [JsonProperty(PropertyName = "credentials"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid Credentials { get; private set; }

        [JsonProperty(PropertyName = "start"), JsonConverter(typeof(VcapDateTimeConverter))] // TODO
        public DateTime Start { get; private set; }

        public VcapComponentBase(
            string argType, int argIndex, Guid argUuid,
            string argHost, Guid argCredentials, DateTime argStart)
        {
            Type        = argType;
            Index       = argIndex;
            Uuid        = argUuid;
            Host        = argHost;
            Credentials = argCredentials;
            Start       = argStart;
        }

        public VcapComponentBase(VcapComponentBase argCopyFrom)
            : this(argCopyFrom.Type, argCopyFrom.Index, argCopyFrom.Uuid,
                   argCopyFrom.Host, argCopyFrom.Credentials, argCopyFrom.Start) { }
    }
}