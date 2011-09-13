namespace CloudFoundry.Net.Types
{
    using System;
    using Newtonsoft.Json;

    public class Instance : JsonBase
    {
        // public Instance() { }

        public Instance(Droplet argDroplet)
        {
            if (null != argDroplet)
            {
                DropletID      = argDroplet.ID;
                InstanceID     = argDroplet.Sha1;
                InstanceIndex  = argDroplet.Index;
                Name           = argDroplet.Name;
                Dir            = IIsName;
                Uris           = argDroplet.Uris;
                Users          = argDroplet.Users;
                Version        = argDroplet.Version;
                MemQuota       = argDroplet.Limits.Mem * (1024 * 1024);
                DiskQuota      = argDroplet.Limits.Disk * (1024 * 1024);
                FdsQuota       = argDroplet.Limits.FDs;
                Runtime        = argDroplet.Runtime;
                Framework      = argDroplet.Framework;
                LogID          = String.Format("(name={0} app_id={1} instance={2} index={3})", argDroplet.Name, argDroplet.ID, argDroplet.Sha1, argDroplet.Index);
                Staged         = argDroplet.Name;
                Sha1           = argDroplet.Sha1;
            }

            State          = InstanceState.STARTING;
            Start          = DateTime.Now.ToString(Constants.JsonDateFormat);
            StateTimestamp = Utility.GetEpochTimestamp();
        }

        [JsonProperty(PropertyName = "droplet_id")]
        public uint DropletID { get; set; }

        [JsonProperty(PropertyName = "instance_id")]
        public string InstanceID { get; set; }

        [JsonProperty(PropertyName = "instance_index")]
        public string InstanceIndex { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "dir")]
        public string Dir { get; set; }

        [JsonProperty(PropertyName = "uris")]
        public string[] Uris { get; set; }

        [JsonProperty(PropertyName = "users")]
        public string[] Users { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "mem_quota")]
        public int MemQuota { get; set; }

        [JsonProperty(PropertyName = "disk_quota")]
        public int DiskQuota { get; set; }

        [JsonProperty(PropertyName = "fds_quota")]
        public int FdsQuota { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "runtime")]
        public string Runtime { get; set; }

        [JsonProperty(PropertyName = "framework")]
        public string Framework { get; set; }

        [JsonProperty(PropertyName = "start")]
        public string Start { get; set; }

        [JsonProperty(PropertyName = "state_timestamp")]
        public int StateTimestamp { get; set; }

        [JsonProperty(PropertyName = "log_id")]
        public string LogID { get; set; }

        [JsonProperty(PropertyName = "port")]
        public ushort Port { get; set; }

        [JsonProperty(PropertyName = "staged")]
        public string Staged { get; set; }

        [JsonProperty(PropertyName = "exit_reason")]
        public string ExitReason { get; set; }

        [JsonProperty(PropertyName = "sha1")]
        public string Sha1 { get; set; }

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
            get { return String.Format("{0}-{1}-{2}", Name, InstanceIndex, InstanceID); }
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