namespace CloudFoundry.Net.Types
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [Serializable]
    public class Application : EntityBase
    {
        private string name;
        private Staging staging;
        private string version;
        private int instances;
        private int? runningInstances;
        private AppResources resources;
        private string state;
        private AppMeta metadata;
        private readonly ObservableCollection<string> uris = new ObservableCollection<string>();
        private readonly ObservableCollection<string> services = new ObservableCollection<string>();        
        private readonly ObservableCollection<string> environment = new ObservableCollection<string>();

        public Application()
        {
            uris.CollectionChanged += UrisChanged;
            services.CollectionChanged += ServicesChanged;
            environment.CollectionChanged += EnvironmentChanged;

            Staging = new Staging();
            Resources = new AppResources();
            MetaData = new AppMeta();
        }

        void EnvironmentChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Environment");
        }

        void ServicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Services");
        }

        void UrisChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Uris");
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
        public ObservableCollection<string> Uris
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
        public ObservableCollection<string> Services
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
        public ObservableCollection<string> Environment
        {
            get { return this.environment; }
        }

        [JsonProperty(PropertyName = "meta")]
        public AppMeta MetaData
        {
            get { return this.metadata; }
            set { this.metadata = value; RaisePropertyChanged("MetaData"); }
        }

        [JsonIgnore]
        public Cloud Parent { get; set; }
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

    [Serializable]
    public class AppMeta : EntityBase
    {
        private int version;
        private long created;

        [JsonProperty(PropertyName = "version")]
        public int Version 
        {
            get { return this.version; }
            set { this.version = value; RaisePropertyChanged("Version"); }
        }

        [JsonProperty(PropertyName = "created")]
        public long Created
        {
            get { return this.created; }
            set { this.created = value; RaisePropertyChanged("Created"); }
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