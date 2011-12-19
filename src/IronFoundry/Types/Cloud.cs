namespace CloudFoundry.Net.Types
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using CloudFoundry.Net.Extensions;

    [Serializable]
    public class Cloud : EntityBase, IEquatable<Cloud>, IMergeable<Cloud>
    {
        private readonly Guid id;
        private string serverName;
        private string hostName;
        private string email;
        private string password;
        private string url;
        private bool isConnected;
        private bool isDisconnected;
        private int timeoutStart;
        private int timeoutStop;
        private string accessToken;

        private SafeObservableCollection<Application> applications = new SafeObservableCollection<Application>();
        private SafeObservableCollection<SystemService> availableServices = new SafeObservableCollection<SystemService>();
        private SafeObservableCollection<ProvisionedService> services = new SafeObservableCollection<ProvisionedService>();

        public Cloud()
        {
            applications.CollectionChanged += (s, e) => RaisePropertyChanged("Applications");
            services.CollectionChanged += (s, e) => RaisePropertyChanged("Services");
            availableServices.CollectionChanged += (s, e) => RaisePropertyChanged("AvailableServices");

            id = Guid.NewGuid();
            TimeoutStart = 600;
            TimeoutStop = 60;
            IsConnected = false;
            IsDisconnected = true;
        }        

        public Guid ID
        {
            get { return id; }
        }

        public string ServerName 
        {
            get { return this.serverName; }
            set { this.serverName = value; RaisePropertyChanged("ServerName"); }
        }

        public string HostName
        {
            get { return this.hostName; }
            set { this.hostName = value; RaisePropertyChanged("HostName"); }
        }

        public string Email
        {
            get { return this.email; }
            set { this.email = value; RaisePropertyChanged("Email"); }
        }

        public string Password
        {
            get { return this.password; }
            set { this.password = value; RaisePropertyChanged("Password"); }
        }

        public string Url
        {
            get { return this.url; }
            set { this.url = value; RaisePropertyChanged("Url"); }
        }

        public int TimeoutStart
        {
            get { return this.timeoutStart; }
            set { this.timeoutStart = value; RaisePropertyChanged("TimeoutStart"); }
        }

        public int TimeoutStop
        {
            get { return this.timeoutStop; }
            set { this.timeoutStop = value; RaisePropertyChanged("TimeoutStop"); }
        }

        public string AccessToken
        {
            get { return this.accessToken; }
            set { 
                this.accessToken = value;
                RaisePropertyChanged("AccessToken");
                if (!String.IsNullOrEmpty(accessToken))
                {
                    this.IsConnected = true;
                    this.IsDisconnected = false;
                }
                else
                {
                    this.IsDisconnected = true;
                    this.IsConnected = false;
                }                
            }
        }

        public bool IsConnected
        {
            get { return this.isConnected; }
            set { this.isConnected = value; RaisePropertyChanged("IsConnected"); }         
        }

        public bool IsDisconnected
        {
            get { return this.isDisconnected; }
            set { this.isDisconnected = value; RaisePropertyChanged("IsDisconnected"); }
        }

        public SafeObservableCollection<Application> Applications 
        {
            get
            {
                if (this.applications == null)
                    this.applications = new SafeObservableCollection<Application>();
                return this.applications; 
            }            
        }

        public SafeObservableCollection<ProvisionedService> Services
        {
            get {
                if (this.services == null)
                    this.services = new SafeObservableCollection<ProvisionedService>();
                return this.services; 
            }
        }

        public SafeObservableCollection<SystemService> AvailableServices
        {
            get {
                if (this.availableServices == null)
                    this.availableServices = new SafeObservableCollection<SystemService>();
                return this.availableServices; 
            }
        }        

        public void Merge(Cloud c)
        {
            this.ServerName = c.ServerName;
            this.HostName = c.HostName;
            this.Email = c.Email;
            this.Password = c.Password;
            this.TimeoutStart = c.TimeoutStart;
            this.TimeoutStop = c.TimeoutStop;
            this.AccessToken = c.AccessToken;
            this.IsConnected = c.IsConnected;
            this.IsDisconnected = c.IsDisconnected;

            this.Applications.Synchronize(c.Applications,new ApplicationEqualityComparer());
            this.Services.Synchronize(c.Services,new ProvisionedServiceEqualityComparer());
            this.AvailableServices.Synchronize(c.AvailableServices, new SystemServiceEqualityComparer());            
        }

        public bool Equals(Cloud other)
        {
            if (null == other)
                return false;

            return this.GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (null == obj)
                return false;

            if (DependencyProperty.UnsetValue == obj)
                return false;

            return Equals(obj as Cloud);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }    
}