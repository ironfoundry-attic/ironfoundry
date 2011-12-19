namespace IronFoundry.Types
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    [Serializable]
    public class Application : EntityBase, IMergeable<Application> 
    {
        private static class VcapStates
        {
            public const string STARTING      = "STARTING";
            public const string STOPPED       = "STOPPED";
            public const string RUNNING       = "RUNNING";
            public const string STARTED       = "STARTED";
            public const string SHUTTING_DOWN = "SHUTTING_DOWN";
            public const string CRASHED       = "CRASHED";
            public const string DELETED       = "DELETED";

            public static bool IsValid(string argState)
            {
                return STARTING == argState ||
                       STOPPED == argState ||
                       RUNNING == argState ||
                       SHUTTING_DOWN == argState ||
                       CRASHED == argState ||
                       DELETED == argState;
            }
        }

        private string name;
        private Staging staging;
        private string version;
        private int instances;
        private int? runningInstances;
        private AppResources resources;
        private string state;
        private readonly SafeObservableCollection<string> uris = new SafeObservableCollection<string>();
        private readonly SafeObservableCollection<string> services = new SafeObservableCollection<string>();        
        private readonly SafeObservableCollection<string> environment = new SafeObservableCollection<string>();
        private readonly SafeObservableCollection<Instance> instanceCollection = new SafeObservableCollection<Instance>();

        public Application()
        {
            uris.CollectionChanged += (s,e) => RaisePropertyChanged("Uris");
            services.CollectionChanged += (s,e) => RaisePropertyChanged("Services");
            environment.CollectionChanged += (s,e) => RaisePropertyChanged("Environment");
            instanceCollection.CollectionChanged += (s,e) => RaisePropertyChanged("InstanceCollection");

            Staging = new Staging();
            Resources = new AppResources();
        }

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get { return this.name; }
            set { this.name = value; RaisePropertyChanged("Name"); }
        }

        [JsonProperty(PropertyName = "staging")]
        public Staging Staging
        {
            get { return this.staging; }
            set { this.staging = value; RaisePropertyChanged("Staging"); }
        }

        [JsonProperty(PropertyName = "uris")]
        public SafeObservableCollection<string> Uris
        {
            get { return this.uris; }
        }

        [JsonProperty(PropertyName = "instances")]
        public int Instances
        {
            get { return this.instances; }
            set { this.instances = value; RaisePropertyChanged("Instances"); }
        }

        [JsonProperty(PropertyName = "runningInstances")]
        public int? RunningInstances
        {
            get { return this.runningInstances; }
            set { this.runningInstances = value; RaisePropertyChanged("RunningInstances"); }
        }

        [JsonProperty(PropertyName = "resources")]
        public AppResources Resources
        {
            get { return this.resources; }
            set { this.resources = value; RaisePropertyChanged("Resources"); }
        }

        [JsonProperty(PropertyName = "state")]
        public string State
        {
            get { return this.state; }
            set { this.state = value; RaisePropertyChanged("State"); }
        }

        [JsonProperty(PropertyName = "services")]
        public SafeObservableCollection<string> Services 
        {
            get { return this.services; }
        }

        [JsonProperty(PropertyName = "version")]
        public string Version
        {
            get { return this.version; }
            set { this.version = value; RaisePropertyChanged("Version"); }
        }

        [JsonProperty(PropertyName = "env")]
        public SafeObservableCollection<string> Environment
        {
            get { return this.environment; }
        }

        [JsonIgnore]
        public SafeObservableCollection<Instance> InstanceCollection
        {
            get { return this.instanceCollection; }
        }
        
        [JsonIgnore]
        public Cloud Parent { get; set; }

        [JsonIgnore]
        public bool IsStarted
        {
            get { return State == VcapStates.STARTED; }
        }

        [JsonIgnore]
        public bool IsStopped
        {
            get { return State == VcapStates.STOPPED; }
        }

        [JsonIgnore]
        public bool CanStart
        {
            get
            {
                return ! (State == VcapStates.RUNNING || State == VcapStates.STARTED || State == VcapStates.STARTING);
            }
        }

        [JsonIgnore]
        public bool CanStop
        {
            get
            {
                return State == VcapStates.RUNNING || State == VcapStates.STARTED || State == VcapStates.STARTING;
            }
        }

        public void Merge(Application obj)
        {
            this.Staging          = obj.Staging;
            this.Resources        = obj.Resources;
            this.Version          = obj.Version;
            this.Instances        = obj.Instances;
            this.RunningInstances = obj.RunningInstances;
            this.State            = obj.State;
            this.Uris.Synchronize(obj.Uris,StringComparer.InvariantCulture);
            this.InstanceCollection.Synchronize(obj.InstanceCollection,new InstanceEqualityComparer());
        }

        public void Start()
        {
            this.State = VcapStates.STARTED;
        }

        public void Stop()
        {
            this.State = VcapStates.STOPPED;
        }
    }

    [Serializable]
    public class Staging : EntityBase
    {
        private string model;
        private string stack;

        [JsonProperty(PropertyName = "model")]
        public string Model
        {
            get { return this.model; }
            set { this.model = value; RaisePropertyChanged("Model"); }
        }

        [JsonProperty(PropertyName = "stack")]
        public string Stack
        {
            get { return this.stack; }
            set { this.stack = value; RaisePropertyChanged("Stack"); }
        }
    }

    [Serializable]
    public class AppResources : EntityBase
    {
        private int memory;
        private int disk;
        private int fds;

        [JsonProperty(PropertyName = "memory")]
        public int Memory
        {
            get { return this.memory; }
            set { this.memory = value; RaisePropertyChanged("Memory"); }
        }

        [JsonProperty(PropertyName = "disk")]
        public int Disk
        {
            get { return this.disk; }
            set { this.disk = value; RaisePropertyChanged("Disk"); }
        }

        [JsonProperty(PropertyName = "fds")]
        public int Fds
        {
            get { return this.fds; }
            set { this.fds = value; RaisePropertyChanged("Fds"); }
        }
    }   

    public class ApplicationEqualityComparer : IEqualityComparer<Application>
    {
        public bool Equals(Application c1, Application c2)
        {
            return c1.Name.Equals(c2.Name);
        }

        public int GetHashCode(Application c)
        {
            return c.Name.GetHashCode();
        }
    }
}