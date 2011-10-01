using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;

namespace CloudFoundry.Net.Types
{
    [Serializable]
    public class Cloud : EntityBase
    {
        private Guid id;
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
        private ObservableCollection<Application> applications;
        private ObservableCollection<AppService> services;

        public Cloud()
        {            
            applications = new ObservableCollection<Application>();
            services = new ObservableCollection<AppService>();
            services.CollectionChanged += ServicesChanged;
            applications.CollectionChanged += ApplicationsChanged;
            id = Guid.NewGuid();
            TimeoutStart = 600;
            TimeoutStop = 60;
            IsConnected = false;
            IsDisconnected = true;
        }        

        public Guid ID
        {
            get { return id; }
            set { id = value; }
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
                if (this.accessToken != null)
                {
                    this.IsConnected = true;
                    this.IsDisconnected = false;
                }
                else
                {
                    this.IsDisconnected = true;
                    this.IsConnected = true;
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
            get { return this.applications; }            
        }

        public ObservableCollection<AppService> Services
        {
            get { return this.services; }
        }

        private void ServicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Services");
        }

        private void ApplicationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Applications");
        }
      
    }
}
