namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
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
    using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
    using System.Threading;
    using System.Windows.Input;

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
        public RelayCommand RemoveApplicationServiceCommand { get; private set; }
        public RelayCommand ProvisionServiceCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }

        private Cloud cloud;
        private Application selectedApplication;
        private ProvisionedService selectedApplicationService;
        private bool isApplicationViewSelected;
        private string overviewErrorMessage;
        private string applicationErrorMessage;
        private IDragSource provisionedServicesSource;
        private IDropTarget applicationServicesTarget;

        private ObservableCollection<ProvisionedService> applicationServices;
        private ObservableCollection<Model.Instance> instances;
        private CloudFoundryProvider provider;
        private bool applicationStarting;
        
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
            RemoveApplicationServiceCommand = new RelayCommand(RemoveApplicationService);
            ProvisionServiceCommand = new RelayCommand(ProvisionService);
            RefreshCommand = new RelayCommand(RefreshApplication, CanExecuteRefresh);
        }

        private void LoadProvider(CloudFoundryProvider provider)
        {
            this.provider = provider;
            instanceTimer.Interval = TimeSpan.FromSeconds(5);
            instanceTimer.Tick += RefreshInstances;
            instanceTimer.Start();
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
            if (e.Cancelled)
                return;
            UpdateInstanceCollection((IEnumerable<StatInfo>)e.Result);
        }

        private void BeginUpdateApplication(object sender, DoWorkEventArgs e)
        {
            ApplicationErrorMessage = string.Empty;
            e.Result = provider.UpdateApplication(SelectedApplication, Cloud);
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
            UpdateInstanceCollection(stats, null);
        }

        private void UpdateInstanceCollection(IEnumerable<StatInfo> stats, RunWorkerCompletedEventArgs e)
        {
            var instances = new ObservableCollection<Model.Instance>();

            foreach (var stat in stats)
            {
                if (e != null && e.Cancelled == true)
                    return;

                if (stat.State.Equals(Types.VcapStates.RUNNING) ||
                    stat.State.Equals(Types.VcapStates.STARTED) ||
                    stat.State.Equals(Types.VcapStates.STARTING))
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
            Messenger.Default.Register<NotificationMessageAction<Cloud>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.SetChangePasswordData))
                        message.Execute(Cloud);
                });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ChangePassword,
                (confirmed) =>
                {
                    if (confirmed)
                    {
                        Messenger.Default.Send(new NotificationMessageAction<ChangePasswordViewModel>(Messages.GetChangePasswordData,
                        (viewModel) =>
                        {
                            var result = provider.ChangePassword(Cloud, viewModel.NewPassword);
                            //this.Cloud.Password = viewModel.NewPassword;                               
                        }));
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
                this.Cloud.AvailableServices.Synchronize(returnCloud.AvailableServices, new SystemServiceEqualityComparer());
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

        public ProvisionedService SelectedApplicationService
        {
            get { return this.selectedApplicationService; }
            set
            {
                this.selectedApplicationService = value;
                RaisePropertyChanged("SelectedApplicationService");
            }
        }

        public bool CanExecuteStart()
        {
            return IsApplicationSelected && !applicationStarting && !(
                   SelectedApplication.State.Equals(Types.VcapStates.RUNNING) ||
                   SelectedApplication.State.Equals(Types.VcapStates.STARTED) ||
                   SelectedApplication.State.Equals(Types.VcapStates.STARTING));
        }

        public bool CanExecuteStopActions()
        {
            return IsApplicationSelected && (
                   SelectedApplication.State.Equals(Types.VcapStates.RUNNING) ||
                   SelectedApplication.State.Equals(Types.VcapStates.STARTED) ||
                   SelectedApplication.State.Equals(Types.VcapStates.STARTING));
        }

        public void Start()
        {
            applicationStarting = true;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, args) => provider.Start(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
            worker.RunWorkerCompleted += (s, args) => { RefreshApplication(); applicationStarting = false; };
            worker.RunWorkerAsync();
        }

        public void Stop()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, args) => provider.Stop(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
            worker.RunWorkerCompleted += (s, args) => RefreshApplication();
            worker.RunWorkerAsync();
        }

        public void Restart()
        {
            Start();
            Stop();          
        }

        public void UpdateAndRestart()
        {
            Restart();
        }

        private bool CanExecuteRefresh()
        {
            return SelectedApplication != null;
        }

        private void RefreshApplication()
        {
            var application = provider.GetApplication(SelectedApplication, Cloud);
            var applicationToReplace = Cloud.Applications.SingleOrDefault((i) => i.Name == application.Name);
            if (applicationToReplace != null)
                applicationToReplace = application;
            SelectedApplication = application;
            CommandManager.InvalidateRequerySuggested();
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

        public bool IsNotApplicationViewSelected
        {
            get { return !this.isApplicationViewSelected; }
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
                RaisePropertyChanged("SelectedApplication");
                RaisePropertyChanged("IsApplicationSelected");

                this.ApplicationServices = new ObservableCollection<ProvisionedService>();
                foreach (var svc in this.selectedApplication.Services)
                    foreach (var appService in Cloud.Services)
                        if (appService.Name.Equals(svc, StringComparison.InvariantCultureIgnoreCase))
                            this.ApplicationServices.Add(appService);

                BackgroundWorker instanceWorker = new BackgroundWorker();
                instanceWorker.DoWork += BeginGetInstances;
                instanceWorker.RunWorkerCompleted += EndGetInstances;
                instanceWorker.WorkerSupportsCancellation = true;
                instanceWorker.RunWorkerAsync();
            }
        }

        private void selectedApplication_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            BackgroundWorker updateApplication = new BackgroundWorker();
            updateApplication.DoWork += BeginUpdateApplication;
            updateApplication.RunWorkerCompleted += EndUpdateApplication; 
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

        private void ProvisionService()
        {
            Messenger.Default.Register<NotificationMessageAction<Cloud>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.SetProvisionServiceData))
                        message.Execute(this.Cloud);
                });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ProvisionService,
                (confirmed) =>
                {
                    if (confirmed)
                    {
                        Messenger.Default.Send(new NotificationMessageAction<ProvisionedServiceViewModel>(Messages.GetProvisionServiceData,
                        (viewModel) =>
                        {
                            var result = provider.CreateService(this.Cloud,viewModel.SelectedSystemService.Vendor, viewModel.Name);
                            var systemServices = provider.GetProvisionedServices(this.Cloud);
                            this.Cloud.Services.Synchronize(systemServices, new ProvisionedServiceEqualityComparer());
                        }));
                    }
                }));
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

        private void RemoveApplicationService()
        {
            if (SelectedApplicationService != null)
            {
                if (SelectedApplication.Services.Contains(SelectedApplicationService.Name))
                {
                    SelectedApplication.Services.Remove(SelectedApplicationService.Name);
                    RaisePropertyChanged("ApplicationServices");
                }
                this.ApplicationServices.Remove(SelectedApplicationService);
                
            }
        }

        #endregion

        #region DragDropProvisionedServices        

        public IDragSource SourceOfProvisionedServices
        {
            get
            {
                if (provisionedServicesSource == null)
                    provisionedServicesSource = new DragSource<ProvisionedService>(GetBindProvisionedServicesDragEffects, GetBindProvisionedServicesData);
                return provisionedServicesSource;
            }
        }    
    
        public IDropTarget ApplicationServiceSink
        {
            get
            {
                if (this.applicationServicesTarget == null)
                    this.applicationServicesTarget = new DropTarget<ProvisionedService>(GetApplicationServicesDropEffects, DropApplicationServices);
                return applicationServicesTarget;
            }
        }

        private System.Windows.DragDropEffects GetBindProvisionedServicesDragEffects(ProvisionedService provisionedService)
        {
            return this.Cloud.Services.Any() ? System.Windows.DragDropEffects.Move : System.Windows.DragDropEffects.None;
        }

        private object GetBindProvisionedServicesData(ProvisionedService provisionedService)
        {
            return provisionedService;
        }

        private void DropApplicationServices(ProvisionedService provisionedService)
        {
            this.ApplicationServices.Add(provisionedService);
            foreach (var service in this.ApplicationServices)
            {
                if (!SelectedApplication.Services.Contains(service.Name))
                {                    
                    SelectedApplication.Services.Add(service.Name);
                    RaisePropertyChanged("ApplicationServices");
                }
            }          
        }

        private System.Windows.DragDropEffects GetApplicationServicesDropEffects(ProvisionedService provisionedService)
        {
            var existingService = ApplicationServices.SingleOrDefault(i => i.Name.Equals(provisionedService.Name, StringComparison.InvariantCultureIgnoreCase));            
            return (existingService != null) ? System.Windows.DragDropEffects.None : System.Windows.DragDropEffects.Move;
        }

        

        #endregion
    }
}
