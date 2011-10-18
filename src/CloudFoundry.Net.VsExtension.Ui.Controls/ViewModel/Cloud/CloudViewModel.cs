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
        public RelayCommand UpdateInstanceCountCommand { get; private set; }
        public RelayCommand ManageApplicationUrlsCommand { get; private set; }
        public RelayCommand RemoveApplicationServiceCommand { get; private set; }
        public RelayCommand ProvisionServiceCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand DeleteApplicationCommand { get; private set; }

        private Cloud cloud;
        private CloudFoundryProvider provider;
        private Application selectedApplication;
        private ProvisionedService selectedApplicationService;
        private bool isApplicationViewSelected;
        private bool applicationStarting;
        private bool isAccountValid;
        private Dispatcher dispatcher;
        private string overviewErrorMessage;
        private string applicationErrorMessage;
        private IDragSource provisionedServicesSource;
        private IDropTarget applicationServicesTarget;
        private ObservableCollection<ProvisionedService> applicationServices;
        private ObservableCollection<Model.Instance> instances;

        public CloudViewModel(Cloud cloud)
        {
            Messenger.Default.Send<NotificationMessageAction<CloudFoundryProvider>>(new NotificationMessageAction<CloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            this.dispatcher = Dispatcher.CurrentDispatcher;
            this.Cloud = cloud;

            ChangePasswordCommand = new RelayCommand(ChangePassword);
            ValidateAccountCommand = new RelayCommand(ValidateAccount, CanExecuteValidateAccount);
            ConnectCommand = new RelayCommand(Connect, CanExecuteConnect);
            DisconnectCommand = new RelayCommand(Disconnect, CanExecuteDisconnect);
            StartCommand = new RelayCommand(Start, CanExecuteStart);
            StopCommand = new RelayCommand(Stop, CanExecuteStopActions);
            RestartCommand = new RelayCommand(Restart, CanExecuteStopActions);
            ManageApplicationUrlsCommand = new RelayCommand(ManageApplicationUrls);
            RemoveApplicationServiceCommand = new RelayCommand(RemoveApplicationService);
            ProvisionServiceCommand = new RelayCommand(ProvisionService);
            RefreshCommand = new RelayCommand(Refresh, CanExecuteRefresh);
            DeleteApplicationCommand = new RelayCommand(DeleteApplication);

            var instanceTimer = new DispatcherTimer();
            instanceTimer.Interval = TimeSpan.FromSeconds(10);
            instanceTimer.Tick += RefreshInstances;
            instanceTimer.Start();
        }

        #region InstanceManagement

        private void RefreshInstances(object sender, EventArgs e)
        {
            if (null != SelectedApplication)
            {
                var worker = new BackgroundWorker();
                worker.DoWork += BeginGetInstances;
                worker.RunWorkerCompleted += EndGetInstances;
                worker.RunWorkerAsync();
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
            var statsResponse = e.Result as ProviderResponse<IEnumerable<StatInfo>>;
            if (statsResponse.Response != null)
                UpdateInstanceCollection(statsResponse.Response);
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

        #endregion

        #region Properties

        public string OverviewErrorMessage
        {
            get { return this.overviewErrorMessage; }
            set { this.overviewErrorMessage = value; RaisePropertyChanged("OverviewErrorMessage");
            if (!String.IsNullOrWhiteSpace(this.overviewErrorMessage))
                {
                    var worker = new BackgroundWorker();
                    worker.DoWork += (s, e) => Thread.Sleep(TimeSpan.FromSeconds(7));
                    worker.RunWorkerCompleted += (s, e) => this.OverviewErrorMessage = string.Empty;
                    worker.RunWorkerAsync();
                }            
            }
        }

        public bool IsAccountValid
        {
            get { return this.isAccountValid; }
            set { this.isAccountValid = value; RaisePropertyChanged("IsAccountValid"); }
        }                  

        public Cloud Cloud
        {
            get { return this.cloud; }
            set { this.cloud = value; RaisePropertyChanged("Cloud"); }
        }

        public string ApplicationErrorMessage
        {
            get { return this.applicationErrorMessage; }
            set { this.applicationErrorMessage = value; RaisePropertyChanged("ApplicationErrorMessage");
                if (!String.IsNullOrWhiteSpace(this.applicationErrorMessage))
                {
                    var worker = new BackgroundWorker();
                    worker.DoWork += (s, e) => Thread.Sleep(TimeSpan.FromSeconds(7));
                    worker.RunWorkerCompleted += (s, e) => this.ApplicationErrorMessage = string.Empty;
                    worker.RunWorkerAsync();
                }
            }
        }

        public ProvisionedService SelectedApplicationService
        {
            get { return this.selectedApplicationService; }
            set { this.selectedApplicationService = value; RaisePropertyChanged("SelectedApplicationService"); }
        }

        public int[] MemoryLimits
        {
            get { return Constants.MemoryLimits; }
        }

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
            set { this.isApplicationViewSelected = value; RaisePropertyChanged("IsApplicationViewSelected"); }
        }

        public ObservableCollection<Model.Instance> Instances
        {
            get { return this.instances; }
            set { this.instances = value; RaisePropertyChanged("Instances"); }
        }

        public ObservableCollection<ProvisionedService> ApplicationServices
        {
            get { return this.applicationServices; }
            set { this.applicationServices = value; RaisePropertyChanged("ApplicationServices"); }
        }

#endregion

        #region CanExecutes

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

        private bool CanExecuteRefresh()
        {
            return SelectedApplication != null;
        }      

        #endregion

        #region Selected Application

        public Application SelectedApplication
        {
            get { return this.selectedApplication; }
            set
            {
                if (this.SelectedApplication != null)
                {
                    this.SelectedApplication.PropertyChanged -= SelectedApplicationPropertyChanged;
                    this.SelectedApplication.Resources.PropertyChanged -= SelectedApplicationPropertyChanged;
                }
                this.selectedApplication = value;
                if (this.selectedApplication != null)
                {
                    this.selectedApplication.PropertyChanged += SelectedApplicationPropertyChanged;
                    this.selectedApplication.Resources.PropertyChanged += SelectedApplicationPropertyChanged;                    

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
                RaisePropertyChanged("SelectedApplication");
                RaisePropertyChanged("IsApplicationSelected");
            }
        }

        private void SelectedApplicationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (s, args) =>
            {
                ApplicationErrorMessage = string.Empty;
                args.Result = provider.UpdateApplication(SelectedApplication, Cloud);
            };
            worker.RunWorkerCompleted += (s, args) =>
            {
                var result = args.Result as ProviderResponse<bool>;
                if (!result.Response)
                    ApplicationErrorMessage = result.Message;
                RefreshApplication();
            };
            worker.RunWorkerAsync();
        }        

        #endregion

        #region Commands

        private void ChangePassword()
        {
            this.IsAccountValid = false;
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
                        Messenger.Default.Send(new NotificationMessageAction<ChangePasswordViewModel>(Messages.GetChangePasswordData, v => this.Cloud.Password = v.NewPassword));
                }));
        }

        private void ProvisionService()
        {
            Messenger.Default.Register<NotificationMessageAction<Cloud>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.SetCreateServiceData))
                        message.Execute(this.Cloud);
                });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.CreateService,
                (confirmed) =>
                {
                    if (confirmed)
                    {
                        Messenger.Default.Send(new NotificationMessageAction<CreateServiceViewModel>(Messages.GetCreateServiceData,
                        (viewModel) =>
                        {
                            var result = provider.GetProvisionedServices(this.Cloud);
                            if (result.Response == null)
                                ApplicationErrorMessage = result.Message;
                            else 
                                this.Cloud.Services.Synchronize(result.Response, new ProvisionedServiceEqualityComparer());
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

        private void Connect()
        {
            this.OverviewErrorMessage = string.Empty;
            var result = this.provider.Connect(this.Cloud);
            if (result.Response != null)
            {
                this.Cloud.AccessToken = result.Response.AccessToken;
                this.Cloud.Services.Synchronize(result.Response.Services, new ProvisionedServiceEqualityComparer());
                this.Cloud.Applications.Synchronize(result.Response.Applications, new ApplicationEqualityComparer());
                this.Cloud.AvailableServices.Synchronize(result.Response.AvailableServices, new SystemServiceEqualityComparer());
            }
            else
            {
                this.OverviewErrorMessage = result.Message;
            }
        }

        private void Disconnect()
        {
            this.Cloud = provider.Disconnect(this.Cloud);
            this.IsAccountValid = false;
        }

        private void ValidateAccount()
        {
            this.OverviewErrorMessage = string.Empty;
            this.IsAccountValid = false;
            var result = this.provider.ValidateAccount(this.Cloud);
            if (result.Response)
                this.IsAccountValid = true;
            else
                this.OverviewErrorMessage = result.Message;
        }

        private void Refresh()
        {
            var worker = new BackgroundWorker();
            SetProgressTitle("Refresh Application");
            worker.DoWork += (s, args) =>
            {
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(50, "Refreshing Application: " + SelectedApplication.Name))));
                var resultString = RefreshApplication();
                if (!String.IsNullOrEmpty(resultString))
                {
                    dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError(resultString))));
                    return;
                }
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(100, "Application Refreshed."))));
            };
            worker.RunWorkerAsync();
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.Progress, c => { }));
        }

        private string RefreshApplication()
        {
            var result = provider.GetApplication(SelectedApplication, Cloud);
            if (result.Response == null)
                return result.Message;
            else
            {
                var application = result.Response;
                
                var applicationToReplace = Cloud.Applications.SingleOrDefault((i) => i.Name == application.Name);
                if (applicationToReplace != null)
                {
                    var index = Cloud.Applications.IndexOf(applicationToReplace);
                    dispatcher.BeginInvoke((Action)(() => {
                        Cloud.Applications[index] = application;
                        SelectedApplication = application;
                    }));                
                }                
                return string.Empty;
            }
        }

        public void DeleteApplication()
        {
            var worker = new BackgroundWorker();
            SetProgressTitle("Delete Application");
            worker.DoWork += (s, args) =>
            {
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(50, "Deleting Application: " + SelectedApplication.Name))));
                var result = provider.Delete(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!result.Response)
                {
                    dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError(result.Message))));
                    return;
                }

                var applicationToRemove = Cloud.Applications.SingleOrDefault((i) => i.Name == SelectedApplication.Name);
                if (applicationToRemove != null)
                {
                    var index = Cloud.Applications.IndexOf(applicationToRemove);
                    dispatcher.BeginInvoke((Action)(() =>
                    {
                        Cloud.Applications.RemoveAt(index);
                        Messenger.Default.Send(new ProgressMessage(100, "Application Deleted."));
                    }));
                }      
            };
            worker.RunWorkerAsync();
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.Progress, c => { }));
        }

        public void Start()
        {
            applicationStarting = true;
            var worker = new BackgroundWorker();
            SetProgressTitle("Starting Application");
            worker.DoWork += (s, args) =>
            {
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(25, "Starting Application: " + SelectedApplication.Name))));

                var result = provider.Start(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!result.Response)
                {
                    dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError(result.Message))));
                    return;
                }

                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(75, "Refreshing Application: " + SelectedApplication.Name))));
                var resultString = RefreshApplication();
                if (!String.IsNullOrEmpty(resultString))
                {
                    dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError(resultString))));
                    return;
                }
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(100, "Application Started."))));

            };
            worker.RunWorkerCompleted += (s, args) => { applicationStarting = false; };
            worker.RunWorkerAsync();
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.Progress, c => { }));
        }

        public void Stop()
        {
            var worker = new BackgroundWorker();
            SetProgressTitle("Stopping Application");
            worker.DoWork += (s, args) =>
            {
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(25, "Stopping Application: " + SelectedApplication.Name))));

                var result = provider.Stop(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!result.Response)
                {
                    dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError(result.Message))));
                    return;
                }

                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(75, "Refreshing Application: " + SelectedApplication.Name))));
                var resultString = RefreshApplication();
                if (!String.IsNullOrEmpty(resultString))
                {
                    dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError(resultString))));
                    return;
                }
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(100, "Application Stopped."))));

            };
            worker.RunWorkerAsync();
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.Progress, c => { }));
        }

        public void Restart()
        {
            var worker = new BackgroundWorker();
            SetProgressTitle("Restarting Application");
            worker.DoWork += (s, args) =>
            {
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(20, "Stopping Application: " + SelectedApplication.Name))));
                var result = provider.Stop(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!result.Response)
                {
                    dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError(result.Message))));
                    return;
                }
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(40, "Refreshing Application: " + SelectedApplication.Name))));
                var resultString = RefreshApplication();
                if (!String.IsNullOrEmpty(resultString))
                {
                    dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError(resultString))));
                    return;
                }
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(60, "Restarting Application: " + SelectedApplication.Name))));
                result = provider.Start(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!result.Response)
                {
                    dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError(result.Message))));
                    return;
                }
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(80, "Refreshing Application: " + SelectedApplication.Name))));
                resultString = RefreshApplication();
                if (!String.IsNullOrEmpty(resultString))
                {
                    dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError(resultString))));
                    return;
                }
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(100, "Application Restarted."))));

            };
            worker.RunWorkerAsync();
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.Progress, c => { }));
        }    

        #endregion

        #region Utility
        
        private void SetProgressTitle(string title)
        {
            Messenger.Default.Register<NotificationMessageAction<string>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.SetProgressData))
                        message.Execute(title);
                });
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
