namespace CloudFoundry.Net.Types
{
    using System;
    using Converters;
    using Newtonsoft.Json;

    public class Instance : JsonBase
    {
        public Instance(Droplet argDroplet)
        {
            if (null != argDroplet)
            {
                DropletID     = argDroplet.ID;
                InstanceID    = Guid.NewGuid();
                InstanceIndex = argDroplet.Index;
                Name          = argDroplet.Name;
                Dir           = IIsName;
                Uris          = argDroplet.Uris;
                Users         = argDroplet.Users;
                Version       = argDroplet.Version;
                MemQuota      = argDroplet.Limits.Mem * (1024 * 1024);
                DiskQuota     = argDroplet.Limits.Disk * (1024 * 1024);
                FdsQuota      = argDroplet.Limits.FDs;
                Runtime       = argDroplet.Runtime;
                Framework     = argDroplet.Framework;
                Staged        = argDroplet.Name;
                Sha1          = argDroplet.Sha1;
                LogID         = String.Format("(name={0} app_id={1} instance={2:N} index={3})", Name, DropletID, InstanceID, InstanceIndex);
            }

            State          = InstanceState.STARTING;
            Start          = DateTime.Now.ToString(Constants.JsonDateFormat);
            StateTimestamp = Utility.GetEpochTimestamp();
        }

        [JsonProperty(PropertyName = "droplet_id")]
        public uint DropletID { get; private set; }

        [JsonProperty(PropertyName = "instance_id"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid InstanceID { get; private set; }

        [JsonProperty(PropertyName = "instance_index")]
        public string InstanceIndex { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "dir")]
        public string Dir { get; private set; }

        [JsonProperty(PropertyName = "uris")]
        public string[] Uris { get; set; }

        [JsonProperty(PropertyName = "users")]
        public string[] Users { get; private set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; private set; }

        [JsonProperty(PropertyName = "mem_quota")]
        public int MemQuota { get; private set; }

        [JsonProperty(PropertyName = "disk_quota")]
        public int DiskQuota { get; private set; }

        [JsonProperty(PropertyName = "fds_quota")]
        public int FdsQuota { get; private set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "runtime")]
        public string Runtime { get; private set; }

        [JsonProperty(PropertyName = "framework")]
        public string Framework { get; private set; }

        [JsonProperty(PropertyName = "start")]
        public string Start { get; private set; }

        [JsonProperty(PropertyName = "state_timestamp")]
        public int StateTimestamp { get; set; }

        [JsonProperty(PropertyName = "log_id")]
        public string LogID { get; private set; }

        [JsonProperty(PropertyName = "port")]
        public ushort Port { get; set; }

        [JsonProperty(PropertyName = "staged")]
        public string Staged { get; private set; }

        [JsonProperty(PropertyName = "exit_reason")]
        public string ExitReason { get; private set; }

        [JsonProperty(PropertyName = "sha1")]
        public string Sha1 { get; private set; }

        [JsonProperty(PropertyName = "host")]
        public string Host { get; set; }

        [JsonIgnore]
        public bool IsStarting
        {
            get { return null != State && State == InstanceState.STARTING; }
        }

        [JsonIgnore]
        public bool IsRunning
        {
            get { return null != State && State == InstanceState.RUNNING; }
        }

        [JsonIgnore]
        public string IIsName
        {
            get { return String.Format("{0}-{1}-{2:N}", Name, InstanceIndex, InstanceID); }
        }

        public static class InstanceState
        {
            public const string STARTING      = "STARTING";
            public const string STOPPED       = "STOPPED";
            public const string RUNNING       = "RUNNING";
            public const string SHUTTING_DOWN = "SHUTTING_DOWN";
            public const string CRASHED       = "CRASHED";
            public const string DELETED       = "DELETED";
        }
    }
}