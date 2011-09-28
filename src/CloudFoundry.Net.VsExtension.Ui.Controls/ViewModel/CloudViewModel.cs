using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using System.Collections.ObjectModel;
using CloudFoundry.Net.Types;
using CloudFoundry.Net.Vmc;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class CloudViewModel : ViewModelBase
    {
        public RelayCommand ChangePasswordCommand { get; private set; }
        public RelayCommand ValidateAccountCommand { get; private set; }
        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand DisconnectCommand { get; private set; }

        public RelayCommand StartCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }
        public RelayCommand RestartCommand { get; private set; }
        public RelayCommand UpdateAndRestartCommand { get; private set; }
        public Cloud Cloud { get; private set; }

        private string name;
        private string state = Types.Instance.InstanceState.STOPPED;
        private int memoryLimit;
        private int instanceCount;
        private ObservableCollection<string> mappedUrls;
        private Application selectedApplication;
        private bool isApplicationViewSelected;
        private ObservableCollection<AppService> provisionedServices;
        private ObservableCollection<AppService> applicationServices;
        private ObservableCollection<Model.Instance> instances;
        private VmcManager manager;


        public CloudViewModel(Cloud cloud)
        {
            ChangePasswordCommand = new RelayCommand(ChangePassword);
            ValidateAccountCommand = new RelayCommand(ValidateAccount, CanExecuteValidateAccount);
            ConnectCommand = new RelayCommand(Connect, () => !this.Connected);
            DisconnectCommand = new RelayCommand(Disconnect, () => this.Connected);
            StartCommand = new RelayCommand(Start, CanExecuteStart);
            StopCommand = new RelayCommand(Stop, CanExecuteStopActions);
            RestartCommand = new RelayCommand(Restart, CanExecuteStopActions);
            UpdateAndRestartCommand = new RelayCommand(UpdateAndRestart, CanExecuteStopActions);

            this.Cloud = cloud;
            manager = new VmcManager();
            if (String.IsNullOrEmpty(cloud.AccessToken))
                cloud.AccessToken = manager.LogIn(cloud).AccessToken;
            this.CloudServices = new ObservableCollection<AppService>(manager.GetProvisionedServices(cloud));
        }

        #region Overview

        private void ChangePassword()
        {
            // Register to initialize data in dialog
            Messenger.Default.Register<NotificationMessageAction<string>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ChangePasswordEmailAddress))
                        message.Execute(this.EMail);
                });

            // Fire message to open dialog
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ChangePassword,
                (confirmed) =>
                {
                    if (confirmed)
                    {
                        // Send Message to grab ViewModel
                        // Process Results from ViewModel
                    }
                }));
        }

        private void ValidateAccount()
        {

        }

        private bool CanExecuteValidateAccount()
        {
            return !String.IsNullOrEmpty(EMail) &&
                   !String.IsNullOrEmpty(Password) &&
                   !String.IsNullOrEmpty(Url);
        }

        private void Connect()
        {
            this.Connected = true;
        }

        private void Disconnect()
        {
            this.Connected = false;
        }

        public bool Connected
        {
            get { return this.Cloud.Connected; }
            set
            {
                this.Cloud.Connected = value;
                RaisePropertyChanged("Connected");
            }
        }

        public string ServerName
        {
            get { return this.Cloud.ServerName; }
            set
            {
                this.Cloud.ServerName = value;
                RaisePropertyChanged("ServerName");
            }
        }

        public string HostName
        {
            get { return this.Cloud.HostName; }
            set
            {
                this.Cloud.HostName = value;
                RaisePropertyChanged("HostName");
            }
        }

        public string EMail
        {
            get { return this.Cloud.Email; }
            set
            {
                this.Cloud.Email = value;
                RaisePropertyChanged("EMail");
            }
        }

        public string Password
        {
            get { return this.Cloud.Password; }
            set
            {
                this.Cloud.Password = value;
                RaisePropertyChanged("Password");
            }
        }

        public string Url
        {
            get { return this.Cloud.Url; }
            set
            {
                this.Cloud.Url = value;
                RaisePropertyChanged("Url");
            }
        }

        public int TimeoutStart
        {
            get { return this.Cloud.TimeoutStart; }
            set
            {
                this.Cloud.TimeoutStart = value;
                RaisePropertyChanged("TimeoutStart");
            }
        }

        public int TimeoutStop
        {
            get { return this.Cloud.TimeoutStop; }
            set
            {
                this.Cloud.TimeoutStop = value;
                RaisePropertyChanged("TimeoutEnd");
            }
        }
        #endregion

        #region Application

        public bool IsApplicationSelected
        {
            get { return this.SelectedApplication != null; }
        }

        public bool IsApplicationViewSelected
        {
            get { return this.isApplicationViewSelected; }
            set
            {
                this.isApplicationViewSelected = value;
                RaisePropertyChanged("IsApplicationViewSelected");
            }
        }

        public void Start()
        {
            this.State = CloudFoundry.Net.Types.Instance.InstanceState.RUNNING;
        }

        public bool CanExecuteStart()
        {
            return !(this.State.Equals(Types.Instance.InstanceState.RUNNING) ||
                   this.State.Equals(Types.Instance.InstanceState.STARTED) ||
                   this.State.Equals(Types.Instance.InstanceState.STARTING));
        }

        public bool CanExecuteStopActions()
        {
            return this.State.Equals(Types.Instance.InstanceState.RUNNING) ||
                   this.State.Equals(Types.Instance.InstanceState.STARTED) ||
                   this.State.Equals(Types.Instance.InstanceState.STARTING);
        }

        public void Stop()
        {
            manager.StopApp(SelectedApplication, Cloud);
            var application = manager.GetAppInfo(SelectedApplication.Name, Cloud);            
            this.SelectedApplication = application;
        }

        public void Restart()
        {
            this.State = CloudFoundry.Net.Types.Instance.InstanceState.RUNNING;
        }

        public void UpdateAndRestart()
        {
            this.State = CloudFoundry.Net.Types.Instance.InstanceState.RUNNING;
        }

        public int[] MemoryLimits { get { return Constants.MemoryLimits; } }

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                RaisePropertyChanged("Name");
            }
        }


        public string State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
                RaisePropertyChanged("State");
            }
        }

        public ObservableCollection<string> MappedUrls
        {
            get { return this.mappedUrls; }
            set { this.mappedUrls = value; RaisePropertyChanged("MappedUrls"); }
        }

        public int InstanceCount
        {
            get { return this.instanceCount; }
            set
            {
                this.instanceCount = value;
                RaisePropertyChanged("InstanceCount");
            }
        }


        public int MemoryLimit
        {
            get
            {
                return this.memoryLimit;
            }
            set
            {
                this.memoryLimit = value;
                RaisePropertyChanged("MemoryLimit");
            }
        }

        public ObservableCollection<Application> Applications
        {
            get { return this.Cloud.Applications; }
        }


        public Application SelectedApplication
        {
            get { return this.selectedApplication; }
            set
            {

                this.selectedApplication = value;
                manager = new VmcManager();
                var stats = manager.GetStats(this.selectedApplication, this.Cloud);
                var instances = new ObservableCollection<Model.Instance>();
                foreach (var stat in stats)
                {
                    var actualstats = stat.Value.Stats;
                    var instance = new Model.Instance()
                    {
                        ID = stat.Key,
                        Cpu = actualstats.Usage.CpuTime / 100,
                        Cores = actualstats.Cores,
                        Memory = Convert.ToInt32(actualstats.Usage.MemoryUsage) / 1024,
                        MemoryQuota = actualstats.MemQuota / 1048576,
                        Disk = Convert.ToInt32(actualstats.Usage.DiskUsage) / 1048576,
                        DiskQuota = actualstats.DiskQuota / 1048576,
                        Host = actualstats.Host,
                        Parent = this.selectedApplication,
                        Uptime = TimeSpan.FromSeconds(Convert.ToInt32(actualstats.Uptime))
                    };
                    instances.Add(instance);
                }
                this.Instances = instances;
                this.Name = selectedApplication.Name;
                this.InstanceCount = selectedApplication.Instances;
                this.MappedUrls = new ObservableCollection<string>(selectedApplication.Uris);
                this.MemoryLimit = selectedApplication.Resources.Memory;
                this.State = selectedApplication.State;
                this.ApplicationServices = new ObservableCollection<AppService>();
                foreach (var svc in this.selectedApplication.Services)
                    foreach (var appService in this.provisionedServices)
                        if (appService.Name.Equals(svc, StringComparison.InvariantCultureIgnoreCase))
                            this.ApplicationServices.Add(appService);

                RaisePropertyChanged("SelectedApplication");
                RaisePropertyChanged("IsApplicationSelected");              
            }
        }

        public ObservableCollection<AppService> CloudServices
        {
            get { return this.provisionedServices; }
            set
            {
                this.provisionedServices = value;
                RaisePropertyChanged("CloudServices");
            }
        }

        public ObservableCollection<Model.Instance> Instances
        {
            get { return this.instances; }
            set
            {
                this.instances = value;
                RaisePropertyChanged("Instances");
            }
        }

        public ObservableCollection<AppService> ApplicationServices
        {
            get { return this.applicationServices; }
            set
            {
                this.applicationServices = value;
                RaisePropertyChanged("ApplicationServices");
            }
        }

        #endregion


    }
}
