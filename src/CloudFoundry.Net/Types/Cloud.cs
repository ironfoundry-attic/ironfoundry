namespace CloudFoundry.Net.Types
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Windows;    

    [Serializable]
    public class Cloud : EntityBase, IEquatable<Cloud>
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

        private ObservableCollection<Application> applications = new ObservableCollection<Application>();
        private ObservableCollection<SystemService> availableServices = new ObservableCollection<SystemService>();
        private ObservableCollection<ProvisionedService> services = new ObservableCollection<ProvisionedService>();

        public Cloud()
        {            
            applications.CollectionChanged += ApplicationsChanged;
            services.CollectionChanged += ServicesChanged;
            availableServices.CollectionChanged += AvailableServicesChanged;

            id = Guid.NewGuid();
            TimeoutStart = 600;
            TimeoutStop = 60;
            IsConnected = false;
            IsDisconnected = true;
        }

        private void AvailableServicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("AvailableServices");
        }

        private void ServicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Services");
        }

        private void ApplicationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Applications");
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

        public ObservableCollection<Application> Applications 
        {
            get
            {
                if (this.applications == null)
                    this.applications = new ObservableCollection<Application>();
                return this.applications; 
            }            
        }

        public ObservableCollection<ProvisionedService> Services
        {
            get {
                if (this.services == null)
                    this.services = new ObservableCollection<ProvisionedService>();
                return this.services; 
            }
        }

        public ObservableCollection<SystemService> AvailableServices
        {
            get {
                if (this.availableServices == null)
                    this.availableServices = new ObservableCollection<SystemService>();
                return this.availableServices; 
            }
        }

        public void ClearServices()
        {
            Services.Clear();
        }

        public void ClearApplications()
        {
            Applications.Clear();
        }

        public void ClearAvailableServices()
        {
            AvailableServices.Clear();
        }

        public bool Equals(Cloud other)
        {
            return other.ID == this.ID;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj == DependencyProperty.UnsetValue)
                return false;
            return ((Cloud)obj).ID == this.ID;
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }    
}