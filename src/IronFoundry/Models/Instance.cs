using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronFoundry.Models
{
    [Serializable]
    public class Instance : EntityBase, IMergeable<Instance>
    {
        private int cores;
        private float cpu;
        private long disk;
        private long diskQuota;
        private string host;
        private int id;
        private long memory;
        private long memoryQuota;
        private Application parent;
        private string state;
        private TimeSpan uptime;

        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                RaisePropertyChanged("Id");
            }
        }

        public string State
        {
            get { return state; }
            set
            {
                state = value;
                RaisePropertyChanged("State");
            }
        }

        public int Cores
        {
            get { return cores; }
            set
            {
                cores = value;
                RaisePropertyChanged("Cores");
            }
        }

        public long MemoryQuota
        {
            get { return memoryQuota; }
            set
            {
                memoryQuota = value;
                RaisePropertyChanged("MemoryQuota");
            }
        }

        public long DiskQuota
        {
            get { return diskQuota; }
            set
            {
                diskQuota = value;
                RaisePropertyChanged("DiskQuota");
            }
        }

        public string Host
        {
            get { return host; }
            set
            {
                host = value;
                RaisePropertyChanged("Host");
            }
        }

        public float Cpu
        {
            get { return cpu; }
            set
            {
                cpu = value;
                RaisePropertyChanged("Cpu");
            }
        }

        public long Memory
        {
            get { return memory; }
            set
            {
                memory = value;
                RaisePropertyChanged("Memory");
            }
        }

        public long Disk
        {
            get { return disk; }
            set
            {
                disk = value;
                RaisePropertyChanged("Disk");
            }
        }

        public TimeSpan Uptime
        {
            get { return uptime; }
            set
            {
                uptime = value;
                RaisePropertyChanged("Uptime");
            }
        }

        public Application Parent
        {
            get { return parent; }
            set
            {
                parent = value;
                RaisePropertyChanged("Parent");
            }
        }

        #region IMergeable<Instance> Members

        public void Merge(Instance obj)
        {
            Cores = obj.Cores;
            MemoryQuota = obj.MemoryQuota;
            DiskQuota = obj.DiskQuota;
            Host = obj.Host;
            Cpu = obj.Cpu;
            Memory = obj.Memory;
            Disk = obj.Disk;
            Uptime = obj.Uptime;
            State = obj.State;
        }

        #endregion
    }

    public class InstanceEqualityComparer : IEqualityComparer<Instance>
    {
        #region IEqualityComparer<Instance> Members

        public bool Equals(Instance c1, Instance c2)
        {
            return c1.Id.Equals(c2.Id);
        }

        public int GetHashCode(Instance c)
        {
            return c.Id.GetHashCode();
        }

        #endregion
    }
}