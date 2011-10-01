using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CloudFoundry.Net.Types
{
    [Serializable]
    public class Application : EntityBase
    {
        private string name;
        private Staging staging;        
        private int instances;
        private int? runningInstances;
        private Resources resources;
        private string state;
        private string version;
        private ObservableCollection<string> services;
        private ObservableCollection<string> uris;
        private ObservableCollection<string> environment;
        private Metadata metadata;

        public Application()
        {
            Staging = new Staging();
            Resources = new Resources();
            Metadata = new Metadata();
            Services = new ObservableCollection<string>();
            Uris = new ObservableCollection<string>();
            Environment = new ObservableCollection<string>();
            Services.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ServicesChanged);
            Uris.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(UrisChanged);
            Environment.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(EnvironmentChanged);
            Staging.PropertyChanged += StagingChanged;
            Resources.PropertyChanged += ResourcesChanged;
            Metadata.PropertyChanged += MetadataChanged;
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
            set { this.uris = value; RaisePropertyChanged("Uris"); }        
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
        public Resources Resources
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
            set { this.services = value; RaisePropertyChanged("Services"); }
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
            set { this.environment = value; RaisePropertyChanged("Environment"); }
        }

        [JsonProperty(PropertyName = "meta")]
        public Metadata Metadata
        {
            get { return this.metadata; }
            set { this.metadata = value; RaisePropertyChanged("MetaData"); }
        }

        [JsonIgnore]
        public Cloud Parent { get; set; }

        private void MetadataChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("Metadata");
        }

        private void ResourcesChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("Resources");
        }

        private void StagingChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("Staging");
        }

        private void EnvironmentChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Environment");
        }

        private void UrisChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Uris");
        }

        private void ServicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Services");
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
    public class Resources : EntityBase
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
    public class Metadata : EntityBase
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
}
