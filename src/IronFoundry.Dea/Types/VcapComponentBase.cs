namespace IronFoundry.Dea.Types
{
    using System;
    using IronFoundry.Misc.Configuration;
    using JsonConverters;
    using Newtonsoft.Json;

    public abstract class VcapComponentBase : Message
    {
        private ServiceCredential credentials;

        [JsonProperty(PropertyName = "type")]
        public string Type { get; private set; }

        [JsonProperty(PropertyName = "index")]
        public string Index { get; private set; }

        [JsonProperty(PropertyName = "uuid"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid Uuid { get; private set; }

        [JsonProperty(PropertyName = "host")]
        public string Host { get; private set; }

        [JsonProperty(PropertyName = "start"), JsonConverter(typeof(VcapDateTimeConverter))]
        public DateTime Start { get; private set; }

        [JsonProperty(PropertyName = "credentials")]
        public string[] CredentialsAry { get; private set; }

        [JsonIgnore]
        public ServiceCredential Credentials
        {
            get { return credentials; }
            private set
            {
                this.credentials = value;
                if (null != this.credentials)
                {
                    CredentialsAry = new string[] { credentials.Username, credentials.Password };
                }
            }
        }

        public VcapComponentBase(
            string type, string index, Guid uuid,
            string host, ServiceCredential credentials, DateTime start)
        {
            Type        = type;
            Index       = index;
            Uuid        = uuid;
            Host        = host;
            Start       = start;
            Credentials = credentials;
        }

        public VcapComponentBase(VcapComponentBase copyFrom)
            : this(copyFrom.Type, copyFrom.Index, copyFrom.Uuid, copyFrom.Host, copyFrom.Credentials, copyFrom.Start) { }
    }
}