namespace IronFoundry.Dea.Types
{
    using System;
    using JsonConverters;
    using Newtonsoft.Json;

    public class Instance : EntityBase
    {
        private bool isEvacuated = false;
        private string logID;

        public Instance() { }

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
                logID         = String.Format("(name={0} app_id={1} instance={2:N} index={3})", Name, DropletID, InstanceID, InstanceIndex);
            }

            State          = VcapStates.STARTING;
            Start          = DateTime.Now.ToString(Constants.JsonDateFormat);
            StateTimestamp = Utility.GetEpochTimestamp();
        }

        [JsonProperty(PropertyName = "droplet_id")]
        public uint DropletID { get;  set; }

        [JsonProperty(PropertyName = "instance_id"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid InstanceID { get;  set; }

        [JsonProperty(PropertyName = "instance_index")]
        public uint InstanceIndex { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get;  set; }

        [JsonProperty(PropertyName = "dir")]
        public string Dir { get;  set; }

        [JsonProperty(PropertyName = "uris")]
        public string[] Uris { get; set; }

        [JsonProperty(PropertyName = "users")]
        public string[] Users { get;  set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get;  set; }

        [JsonProperty(PropertyName = "mem_quota")]
        public int MemQuota { get;  set; }

        [JsonProperty(PropertyName = "disk_quota")]
        public int DiskQuota { get;  set; }

        [JsonProperty(PropertyName = "fds_quota")]
        public int FdsQuota { get;  set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get;  set; }

        [JsonProperty(PropertyName = "runtime")]
        public string Runtime { get;  set; }

        [JsonProperty(PropertyName = "framework")]
        public string Framework { get;  set; }

        [JsonProperty(PropertyName = "start")]
        public string Start { get;  set; }

        [JsonProperty(PropertyName = "state_timestamp")]
        public int StateTimestamp { get; set; }

        [JsonProperty(PropertyName = "port")]
        public ushort Port { get; set; }

        [JsonProperty(PropertyName = "staged")]
        public string Staged { get;  set; }

        [JsonProperty(PropertyName = "exit_reason")]
        public string ExitReason { get;  set; }

        [JsonIgnore]
        public string LogID
        {
            get { return logID; }
        }

        [JsonIgnore]
        public bool HasExitReason
        {
            get { return false == String.IsNullOrWhiteSpace(ExitReason); }
        }

        [JsonProperty(PropertyName = "sha1")]
        public string Sha1 { get;  set; }

        [JsonProperty(PropertyName = "host")]
        public string Host { get; set; }

        [JsonIgnore]
        public bool IsStarting
        {
            get { return null != State && State == VcapStates.STARTING; }
        }

        [JsonIgnore]
        public bool IsRunning
        {
            get { return null != State && State == VcapStates.RUNNING; }
        }

        [JsonIgnore]
        public bool IsStartingOrRunning
        {
            get { return null != State && (State == VcapStates.RUNNING || State == VcapStates.STARTING); }
        }

        [JsonIgnore]
        public bool IsCrashed
        {
            get { return null != State && State == VcapStates.CRASHED; }
        }

        [JsonIgnore]
        public bool IsEvacuated
        {
            get { return isEvacuated; }
        }

        [JsonIgnore]
        public string IIsName
        {
            get { return String.Format("{0}-{1}-{2:N}", Name, InstanceIndex, InstanceID); }
        }

        [JsonIgnore]
        public bool StopProcessed { get;  set; }

        [JsonIgnore]
        public bool IsNotified { get; set; }

        public void Crashed()
        {
            ExitReason = State = VcapStates.CRASHED;
            StateTimestamp = Utility.GetEpochTimestamp();
        }

        public void DeaEvacuation()
        {
            ExitReason = "DEA_EVACUATION";
        }

        public void DeaShutdown()
        {
            if (VcapStates.CRASHED != State)
            {
                ExitReason = "DEA_SHUTDOWN";
            }
        }

        public void Evacuated()
        {
            isEvacuated = true;
        }

        public void OnDeaStop()
        {
            if (State == VcapStates.STARTING || State == VcapStates.RUNNING)
            {
                ExitReason = VcapStates.STOPPED;
            }

            if (State == VcapStates.CRASHED)
            {
                State = VcapStates.DELETED;
                StopProcessed = false;
            }
        }

        public void DeaStopComplete()
        {
            StopProcessed = true;
        }

        public void OnDeaStart()
        {
            State = VcapStates.RUNNING;
        }

        public void UpdateState(string argNewState)
        {
            if (VcapStates.IsValid(argNewState))
            {
                State = argNewState;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}