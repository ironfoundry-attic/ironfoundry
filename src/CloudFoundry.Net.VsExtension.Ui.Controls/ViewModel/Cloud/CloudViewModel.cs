using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using System.Collections.ObjectModel;
using CloudFoundry.Net.Types;
using CloudFoundry.Net.Vmc;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;

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

        private Cloud cloud;        
        private Application selectedApplication;
        private bool isApplicationViewSelected;
        
        private ObservableCollection<AppService> provisionedServices;
        private ObservableCollection<AppService> applicationServices;
        private ObservableCollection<Model.Instance> instances;
        private VmcManager manager = new VmcManager();

        BackgroundWorker getProvisionedServices = new BackgroundWorker();
        BackgroundWorker getInstances = new BackgroundWorker();

        public CloudViewModel(Cloud cloud)
        {            
            this.Cloud = cloud;
            
            InitializeCommands();
            InitializeData();
        }               

        private void InitializeCommands()
        {
            ChangePasswordCommand = new RelayCommand(ChangePassword);
            ValidateAccountCommand = new RelayCommand(ValidateAccount, CanExecuteValidateAccount);
            ConnectCommand = new RelayCommand(Connect, CanExecuteConnect);
            DisconnectCommand = new RelayCommand(Disconnect, CanExecuteDisconnect);
            StartCommand = new RelayCommand(Start, CanExecuteStart);
            StopCommand = new RelayCommand(Stop, CanExecuteStopActions);
            RestartCommand = new RelayCommand(Restart, CanExecuteStopActions);
            UpdateAndRestartCommand = new RelayCommand(UpdateAndRestart, CanExecuteStopActions);
        }

        private void InitializeData()
        {
            Cloud.PropertyChanged += new PropertyChangedEventHandler(Cloud_PropertyChanged);
            getProvisionedServices.DoWork += new DoWorkEventHandler(getProvisionedServices_DoWork);
            getProvisionedServices.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getProvisionedServices_RunWorkerCompleted);
            getInstances.DoWork += new DoWorkEventHandler(getInstances_DoWork);
            getInstances.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getInstances_RunWorkerCompleted);
        }

        private void Cloud_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AccessToken" && !String.IsNullOrEmpty(Cloud.AccessToken))
            {
                getProvisionedServices.RunWorkerAsync();
            }
        }                

        private void getProvisionedServices_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = manager.GetProvisionedServices(Cloud);
        }

        private void getProvisionedServices_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.CloudServices = new ObservableCollection<AppService>(e.Result as List<AppService>);
        }

        private void getInstances_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = manager.GetStats(SelectedApplication, Cloud);
        }

        private void getInstances_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var stats = e.Result as SortedDictionary<int, StatInfo>;
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
        }

        #region Overview

        private void ChangePassword()
        {
            // Register to initialize data in dialog
            Messenger.Default.Register<NotificationMessageAction<string>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ChangePasswordEmailAddress))
                        message.Execute(Cloud.Email);
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
            return !String.IsNullOrEmpty(Cloud.Email) &&
                   !String.IsNullOrEmpty(Cloud.Password) &&
                   !String.IsNullOrEmpty(Cloud.Url);
        }

        private bool CanExecuteConnect()
        {
            return Cloud.IsDisconnected;
        }

        private bool CanExecuteDisconnect()
        {
            return Cloud.IsConnected;
        }

        private void Connect()
        {
            
        }

        private void Disconnect()
        {
            
        }

        public Cloud Cloud
        {
            get { return this.cloud; }
            set { this.cloud = value; RaisePropertyChanged("Cloud"); }
        }
        
        #endregion

        #region Application        

        public bool CanExecuteStart()
        {
            return IsApplicationSelected && !(
                   SelectedApplication.State.Equals(Types.Instance.InstanceState.RUNNING) ||
                   SelectedApplication.State.Equals(Types.Instance.InstanceState.STARTED) ||
                   SelectedApplication.State.Equals(Types.Instance.InstanceState.STARTING));
        }

        public bool CanExecuteStopActions()
        {            
            return IsApplicationSelected && (
                   SelectedApplication.State.Equals(Types.Instance.InstanceState.RUNNING) ||
                   SelectedApplication.State.Equals(Types.Instance.InstanceState.STARTED) ||
                   SelectedApplication.State.Equals(Types.Instance.InstanceState.STARTING));
        }
        
        public void Start()
        {
            manager.StartApp(SelectedApplication, Cloud);
        }

        public void Stop()
        {
            manager.StopApp(SelectedApplication, Cloud);

        }

        public void Restart()
        {
            manager.RestartApp(SelectedApplication, Cloud);
        }

        public void UpdateAndRestart()
        {
            
        }

        public int[] MemoryLimits { get { return Constants.MemoryLimits; } }        

        public ObservableCollection<Application> Applications
        {
            get { return this.Cloud.Applications; }
        }

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

        public Application SelectedApplication
        {
            get { return this.selectedApplication; }
            set
            {
                this.selectedApplication = value;
                this.selectedApplication.PropertyChanged += new PropertyChangedEventHandler(selectedApplication_PropertyChanged);
                this.ApplicationServices = new ObservableCollection<AppService>();
                foreach (var svc in this.selectedApplication.Services)
                    foreach (var appService in this.provisionedServices)
                        if (appService.Name.Equals(svc, StringComparison.InvariantCultureIgnoreCase))
                            this.ApplicationServices.Add(appService);

                RaisePropertyChanged("SelectedApplication");
                RaisePropertyChanged("IsApplicationSelected");
                getInstances.RunWorkerAsync();
            }
        }

        void selectedApplication_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Services")
            {
                this.ApplicationServices = new ObservableCollection<AppService>();
                foreach (var svc in this.selectedApplication.Services)
                    foreach (var appService in this.provisionedServices)
                        if (appService.Name.Equals(svc, StringComparison.InvariantCultureIgnoreCase))
                            this.ApplicationServices.Add(appService);
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
