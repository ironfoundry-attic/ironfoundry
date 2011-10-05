﻿namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Threading;
    using CloudFoundry.Net.Types;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;

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

        private ObservableCollection<ProvisionedService> applicationServices;
        private ObservableCollection<Model.Instance> instances;
        private CloudFoundryProvider provider;

        BackgroundWorker getInstances = new BackgroundWorker();
        BackgroundWorker updateApplication = new BackgroundWorker();
        DispatcherTimer instanceTimer = new DispatcherTimer();

        public CloudViewModel(Cloud cloud)
        {
            this.Cloud = cloud;
            Messenger.Default.Send<NotificationMessageAction<CloudFoundryProvider>>(new NotificationMessageAction<CloudFoundryProvider>(Messages.GetCloudFoundryProvider, LoadProvider));
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

        private void LoadProvider(CloudFoundryProvider provider)
        {
            this.provider = provider;
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
            if (null != SelectedApplication)
            {
                var stats = provider.GetStats(SelectedApplication, Cloud);
                UpdateInstanceCollection(stats);
            }
        }

        private void BeginGetInstances(object sender, DoWorkEventArgs e)
        {
            e.Result = provider.GetStats(SelectedApplication, Cloud);
        }

        private void EndGetInstances(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateInstanceCollection((IEnumerable<StatInfo>)e.Result);
        }

        private void BeginUpdateApplication(object sender, DoWorkEventArgs e)
        {
            ApplicationErrorMessage = string.Empty;
            e.Result = provider.UpdateApplicationSettings(SelectedApplication, Cloud);
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

        private void UpdateInstanceCollection(IEnumerable<StatInfo> stats)
        {
            var instances = new ObservableCollection<Model.Instance>();
            foreach (var stat in stats)
            {
                if (stat.State.Equals(Types.Instance.InstanceState.RUNNING) ||
                    stat.State.Equals(Types.Instance.InstanceState.STARTED) ||
                    stat.State.Equals(Types.Instance.InstanceState.STARTING))
                {
                    var actualstats = stat.Stats;
                    var instance = new Model.Instance()
                    {
                        ID = stat.ID,
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
            Cloud returnCloud = provider.Connect(this.Cloud);
            if (returnCloud != null)
            {
                this.Cloud.AccessToken = returnCloud.AccessToken;
                this.Cloud.Applications.Synchronize(returnCloud.Applications, new ApplicationEqualityComparer());
                this.Cloud.Services.Synchronize(returnCloud.Services, new ProvisionedServiceEqualityComparer());
            }
        }

        private void Disconnect()
        {
            this.Cloud = provider.Disconnect(this.Cloud);
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
            provider.Start(SelectedApplication, Cloud);
            RefreshApplication();
        }

        public void Stop()
        {
            provider.Stop(SelectedApplication, Cloud);
            RefreshApplication();
        }

        public void Restart()
        {
            provider.Restart(SelectedApplication, Cloud);
            RefreshApplication();
        }

        public void UpdateAndRestart()
        {
            provider.UpdateAndRestart(SelectedApplication, Cloud);
        }

        private void RefreshApplication()
        {
            var application = provider.GetApplication(SelectedApplication, Cloud);
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

                this.ApplicationServices = new ObservableCollection<ProvisionedService>();
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

        public ObservableCollection<ProvisionedService> ApplicationServices
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
                        message.Execute(this.SelectedApplication.Uris.DeepCopy());
                });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ManageApplicationUrls,
                (confirmed) =>
                {
                    if (confirmed)
                    {
                        Messenger.Default.Send(new NotificationMessageAction<ManageApplicationUrlsViewModel>(Messages.GetManageApplicationUrlsData,
                            (viewModel) =>
                            {
                                this.SelectedApplication.Uris.Synchronize(viewModel.Urls, StringComparer.InvariantCultureIgnoreCase);
                            }));
                    }
                }));
        }

        #endregion
    }
}