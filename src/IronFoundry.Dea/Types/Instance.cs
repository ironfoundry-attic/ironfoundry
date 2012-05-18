namespace IronFoundry.Dea.Types
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using IronFoundry.Dea.WindowsJobObjects;
    using JsonConverters;
    using Newtonsoft.Json;

    /*
     * TODO: should probably separate out this "on the wire" class from one used to track data
     */
    public class Instance : EntityBase, IDisposable
    {
        private const int MaxUsageSamples = 30;
        private readonly LinkedList<Usage> usageHistory = new LinkedList<Usage>();
        private readonly DateTime startDate;

        private readonly JobObject jobObject = new JobObject();

        private bool isEvacuated = false;
        private string logID;

        public Instance()
        {
            this.jobObject.DieOnUnhandledException = true;
            this.jobObject.ActiveProcessesLimit = 10;
        }

        public Instance(string appDir, Droplet droplet) : this()
        {
            if (null != droplet)
            {
                DropletID      = droplet.ID;
                InstanceID     = Guid.NewGuid();
                InstanceIndex  = droplet.InstanceIndex;
                Name           = droplet.Name;
                Uris           = droplet.Uris;
                Users          = droplet.Users;
                Version        = droplet.Version;
                MemQuotaBytes  = droplet.Limits.MemoryMB * (1024*1024);
                DiskQuotaBytes = droplet.Limits.DiskMB * (1024*1024);
                FdsQuota       = droplet.Limits.FDs;
                Runtime        = droplet.Runtime;
                Framework      = droplet.Framework;
                Staged         = droplet.Name;
                Sha1           = droplet.Sha1;
                logID          = String.Format("(name={0} app_id={1} instance={2:N} index={3})", Name, DropletID, InstanceID, InstanceIndex);
                Staged         = String.Format("{0}-{1}-{2:N}", Name, InstanceIndex, InstanceID);
                Dir            = Path.Combine(appDir, Staged);
            }

            State          = VcapStates.STARTING;
            startDate      = DateTime.Now;
            Start          = startDate.ToJsonString();
            StateTimestamp = Utility.GetEpochTimestamp();
        }

        [JsonIgnore]
        public DateTime StartDate
        {
            get { return startDate; }
        }

        [JsonProperty(PropertyName = "droplet_id")]
        public uint DropletID { get; set; }

        [JsonProperty(PropertyName = "instance_id"), JsonConverter(typeof(VcapGuidConverter))]
        public Guid InstanceID { get; set; }

        [JsonProperty(PropertyName = "instance_index")]
        public uint InstanceIndex { get; set; }

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
        public uint MemQuotaBytes { get; set; }

        [JsonProperty(PropertyName = "disk_quota")]
        public uint DiskQuotaBytes { get; set; }

        [JsonProperty(PropertyName = "fds_quota")]
        public uint FdsQuota { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "runtime")]
        public string Runtime { get; set; }

        [JsonProperty(PropertyName = "framework")]
        public string Framework { get; set; }

        [JsonProperty(PropertyName = "start")]
        public string Start { get; private set; }

        [JsonProperty(PropertyName = "state_timestamp")]
        public int StateTimestamp { get; set; }

        [JsonProperty(PropertyName = "port")]
        public ushort Port { get; set; }

        [JsonProperty(PropertyName = "staged")]
        public string Staged { get; set; }

        [JsonProperty(PropertyName = "exit_reason")]
        public string ExitReason { get; set; }

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
        public string Sha1 { get; set; }

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
        public bool StopProcessed { get; set; }

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
            StateTimestamp = Utility.GetEpochTimestamp();
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

        public void SetWorkerProcess(Process instanceWorkerProcess)
        {
            if (null != instanceWorkerProcess &&
                false == instanceWorkerProcess.HasExited &&
                false == jobObject.HasProcess(instanceWorkerProcess))
            {
                // TODO add limits, priority class
                jobObject.AddProcess(instanceWorkerProcess);
            }
        }

        [JsonIgnore]
        public bool CanGatherStats
        {
            get { return this.IsStarting || this.IsRunning; }
        }

        [JsonIgnore]
        public long TotalProcessorTicks
        {
            get
            {
                long rv = 0;
                if (null != jobObject)
                {
                    rv = jobObject.TotalProcessorTime.Ticks;
                }
                return rv;
            }
        }

        [JsonIgnore]
        public long MostRecentProcessorTicks
        {
            get
            {
                long rv = 0;
                if (usageHistory.Count > 0)
                {
                    Usage mostRecent = usageHistory.First.Value;
                    rv = mostRecent.TotalCpuTicks;
                }
                return rv;
            }
        }

        [JsonIgnore]
        public long WorkingSetMemory
        {
            get
            {
                long rv = 0;
                if (null != jobObject)
                {
                    rv = jobObject.WorkingSetMemory;
                }
                return rv;
            }
        }

        [JsonIgnore]
        public Usage MostRecentUsage
        {
            get
            {
                Usage rv = null;
                if (usageHistory.Count > 0)
                {
                    rv = usageHistory.First.Value;
                }
                return rv;
            }
        }

        public void AddUsage(long memBytes, float cpu, long diskBytes, long currentTicks)
        {
            var newUsage = new Usage();
            newUsage.Time = DateTime.Now;
            newUsage.Cpu = cpu;
            newUsage.MemoryUsageKB = memBytes / 1024;
            newUsage.DiskUsageBytes = diskBytes;
            newUsage.TotalCpuTicks = currentTicks;

            usageHistory.AddFirst(newUsage);

            if (usageHistory.Count > MaxUsageSamples)
            {
                usageHistory.RemoveLast();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.jobObject != null)
                {
                    this.jobObject.Dispose();
                }
            }
        }
    }
}