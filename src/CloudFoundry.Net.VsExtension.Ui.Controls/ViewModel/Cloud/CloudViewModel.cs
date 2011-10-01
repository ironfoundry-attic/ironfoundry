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
using System.Linq;
using System.Windows.Threading;

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
        public RelayCommand UpdateInstanceCountCommand { get; private set; }

        public RelayCommand ManageApplicationUrlsCommand { get; private set; }

        private Cloud cloud;
        private Application selectedApplication;
        private bool isApplicationViewSelected;
        private string overviewErrorMessage;
        private string applicationErrorMessage;

        private ObservableCollection<AppService> applicationServices;
        private ObservableCollection<Model.Instance> instances;
        private VcapClient manager = new VcapClient();

        BackgroundWorker getInstances = new BackgroundWorker();
        BackgroundWorker updateApplication = new BackgroundWorker();
        DispatcherTimer instanceTimer = new DispatcherTimer();

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
            ManageApplicationUrlsCommand = new RelayCommand(ManageApplicationUrls);
        }

        private void InitializeData()
        {
            instanceTimer.Interval = TimeSpan.FromSeconds(5);
            instanceTimer.Tick += RefreshInstances;
            instanceTimer.Start();            
            getInstances.DoWork += BeginGetInstances;
            getInstances.RunWorkerCompleted += EndGetInstances;
            updateApplication.DoWork += BeginUpdateApplication;
            updateApplication.RunWorkerCompleted += EndUpdateApplication;            
        }        

        private void RefreshInstances(object sender, EventArgs e)
        {
            var stats = manager.GetStats(SelectedApplication, Cloud);
            UpdateInstanceCollection(stats);
        }

        private void BeginGetInstances(object sender, DoWorkEventArgs e)
        {
            e.Result = manager.GetStats(SelectedApplication, Cloud);
        }

        private void EndGetInstances(object sender, RunWorkerCompletedEventArgs e)
        {
            var stats = e.Result as SortedDictionary<int, StatInfo>;
            UpdateInstanceCollection(stats);
        }

        private void BeginUpdateApplication(object sender, DoWorkEventArgs e)
        {
            ApplicationErrorMessage = string.Empty;
            e.Result = manager.UpdateApplicationSettings(SelectedApplication, Cloud);
        }

        private void EndUpdateApplication(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                VcapResponse response = e.Result as VcapResponse;
                ApplicationErrorMessage = string.Format("{0} - (Error: {1})", response.Description, response.Code);
            }
            else
            {
                ApplicationErrorMessage = string.Empty;
            }
            RefreshApplication();
        }

        private void UpdateInstanceCollection(SortedDictionary<int, StatInfo> stats)
        {
            var instances = new ObservableCollection<Model.Instance>();
            foreach (var stat in stats)
            {
                if (stat.Value.State.Equals(Types.Instance.InstanceState.RUNNING) ||
                    stat.Value.State.Equals(Types.Instance.InstanceState.STARTED) ||
                    stat.Value.State.Equals(Types.Instance.InstanceState.STARTING))
                {
                    var actualstats = stat.Value.Stats;
                    var instance = new Model.Instance()
                    {
                        ID = stat.Key,
                        Cores = actualstats.Cores,
                        MemoryQuota = actualstats.MemQuota / 1048576,
                        DiskQuota = actualstats.DiskQuota / 1048576,
                        Host = actualstats.Host,
                        Parent = this.selectedApplication,
                        Uptime = TimeSpan.FromSeconds(Convert.ToInt32(actualstats.Uptime))
                    };
                    if (actualstats.Usage != null)
                    {
                        instance.Cpu = actualstats.Usage.CpuTime / 100;
                        instance.Memory = Convert.ToInt32(actualstats.Usage.MemoryUsage) / 1024;
                        instance.Disk = Convert.ToInt32(actualstats.Usage.DiskUsage) / 1048576;
                    }
                    instances.Add(instance);
                }
            }
            this.Instances = instances;
        }

        #region Overview

        public string OverviewErrorMessage
        {
            get { return this.overviewErrorMessage; }
            set { 
                this.overviewErrorMessage = value;
                RaisePropertyChanged("OverviewErrorMessage");
            }
        }

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

        public string ApplicationErrorMessage
        {
            get { return this.applicationErrorMessage; }
            set
            {
                this.applicationErrorMessage = value;
                RaisePropertyChanged("ApplicationErrorMessage");
            }
        }

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
            RefreshApplication();
        }

        public void Stop()
        {
            manager.StopApp(SelectedApplication, Cloud);
            RefreshApplication();
        }

        public void Restart()
        {
            manager.RestartApp(SelectedApplication, Cloud);
            RefreshApplication();
        }

        public void UpdateAndRestart()
        {
            manager.UpdateApplicationSettings(SelectedApplication, Cloud);
            Restart();
        }

        private void RefreshApplication()
        {
            var application = manager.GetAppInfo(SelectedApplication.Name, Cloud);
            var applicationToReplace = Cloud.Applications.SingleOrDefault((i) => i.Name == application.Name);
            if (applicationToReplace != null)
                applicationToReplace = application;
            SelectedApplication = application;
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
                if (this.SelectedApplication != null)
                {
                    this.SelectedApplication.PropertyChanged -= selectedApplication_PropertyChanged;
                    this.SelectedApplication.Resources.PropertyChanged -= selectedApplication_PropertyChanged;
                }
                this.selectedApplication = value;
                this.selectedApplication.PropertyChanged += selectedApplication_PropertyChanged;
                this.selectedApplication.Resources.PropertyChanged += selectedApplication_PropertyChanged;

                this.ApplicationServices = new ObservableCollection<AppService>();
                foreach (var svc in this.selectedApplication.Services)
                    foreach (var appService in Cloud.Services)
                        if (appService.Name.Equals(svc, StringComparison.InvariantCultureIgnoreCase))
                            this.ApplicationServices.Add(appService);

                RaisePropertyChanged("SelectedApplication");
                RaisePropertyChanged("IsApplicationSelected");
                getInstances.RunWorkerAsync();
            }
        }

        private void selectedApplication_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            updateApplication.RunWorkerAsync();
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

        private void ManageApplicationUrls()
        {
            Messenger.Default.Register<NotificationMessageAction<ObservableCollection<string>>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.SetManageApplicationUrlsData))
                        message.Execute(this.SelectedApplication.Uris);
                });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ManageApplicationUrls,
                (confirmed) =>
                {
                    if (confirmed)
                    {
                        Messenger.Default.Send(new NotificationMessageAction<ManageApplicationUrlsViewModel>(Messages.GetManageApplicationUrlsData,
                            (viewModel) =>
                            {
                                this.SelectedApplication.Uris = viewModel.Urls;
                            }));
                    }
                }));
        }

        #endregion
    }
}
