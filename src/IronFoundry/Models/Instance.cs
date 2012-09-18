using System;
using System.Collections.Generic;

namespace IronFoundry.Types
{ 
    [Serializable]
    public class Instance : EntityBase, IMergeable<Instance>
    {
        private int id;
        private string state;
        private int cores;
        private long memoryQuota;
        private long diskQuota;
        private string host;
        private float cpu;
        private long memory;
        private long disk;
        private TimeSpan uptime;
        private Application parent;

        public int ID {
            get { return this.id; }
            set { this.id = value; RaisePropertyChanged("ID"); }
        }
        public string State
        {
            get { return this.state; }
            set { this.state = value; RaisePropertyChanged("State"); }
        }
        public int Cores
        {
            get { return this.cores; }
            set { this.cores = value; RaisePropertyChanged("Cores"); }
        }
        public long MemoryQuota
        {
            get { return this.memoryQuota; }
            set { this.memoryQuota = value; RaisePropertyChanged("MemoryQuota"); }
        }
        public long DiskQuota
        {
            get { return this.diskQuota; }
            set { this.diskQuota = value; RaisePropertyChanged("DiskQuota"); }
        }
        public string Host
        {
            get { return this.host; }
            set { this.host = value; RaisePropertyChanged("Host"); }
        }
        public float Cpu
        {
            get { return this.cpu; }
            set { this.cpu = value; RaisePropertyChanged("Cpu"); }
        }
        public long Memory
        {
            get { return this.memory; }
            set { this.memory = value; RaisePropertyChanged("Memory"); }
        }
        public long Disk
        {
            get { return this.disk; }
            set { this.disk = value; RaisePropertyChanged("Disk"); }
        }
        public TimeSpan Uptime
        {
            get { return this.uptime; }
            set { this.uptime = value; RaisePropertyChanged("Uptime"); }
        }
        public Application Parent
        {
            get { return this.parent; }
            set { this.parent = value; RaisePropertyChanged("Parent"); }
        }

        public void Merge(Instance obj)
        {
            this.Cores = obj.Cores;
            this.MemoryQuota = obj.MemoryQuota;
            this.DiskQuota = obj.DiskQuota;
            this.Host = obj.Host;
            this.Cpu = obj.Cpu;
            this.Memory = obj.Memory;
            this.Disk = obj.Disk;
            this.Uptime = obj.Uptime;
            this.State = obj.State;
        }
    }

    public class InstanceEqualityComparer : IEqualityComparer<Instance>
    {
        public bool Equals(Instance c1, Instance c2)
        {
            return c1.ID.Equals(c2.ID);
        }

        public int GetHashCode(Instance c)
        {
            return c.ID.GetHashCode();
        }
    }
}
