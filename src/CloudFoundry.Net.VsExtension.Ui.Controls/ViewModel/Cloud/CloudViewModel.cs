using CloudFoundry.Net.Extensions;
using CloudFoundry.Net.Utilities;
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
        public RelayCommand ManageApplicationUrlsCommand { get; private set; }
        public RelayCommand RemoveApplicationServiceCommand { get; private set; }
        public RelayCommand ProvisionServiceCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand DeleteApplicationCommand { get; private set; }

        private Cloud cloud;
        private ICloudFoundryProvider provider;
        private Application selectedApplication;
        private ProvisionedService selectedApplicationService;
        private bool isApplicationViewSelected;
        private bool applicationStarting;
        private bool isAccountValid;
        private readonly Dispatcher dispatcher;
        private string overviewErrorMessage;
        private string applicationErrorMessage;
        private IDragSource provisionedServicesSource;
        private IDropTarget applicationServicesTarget;
        private ObservableCollection<ProvisionedService> applicationServices;

        public CloudViewModel(Cloud cloud)
        {
            Messenger.Default.Send(new NotificationMessageAction<ICloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
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
            ProvisionServiceCommand = new RelayCommand(CreateService);
            RefreshCommand = new RelayCommand(Refresh, CanExecuteRefresh);
            DeleteApplicationCommand = new RelayCommand(DeleteApplication);

            var instanceTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(3)};
            instanceTimer.Tick += RefreshApplication;
            instanceTimer.Start();
        }        

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

        public string ApplicationErrorMessage
        {
            get { return this.applicationErrorMessage; }
            set
            {
                this.applicationErrorMessage = value; RaisePropertyChanged("ApplicationErrorMessage");
                if (!String.IsNullOrWhiteSpace(this.applicationErrorMessage))
                {
                    var worker = new BackgroundWorker();
                    worker.DoWork += (s, e) => Thread.Sleep(TimeSpan.FromSeconds(7));
                    worker.RunWorkerCompleted += (s, e) => this.ApplicationErrorMessage = string.Empty;
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
                    foreach (var appService in 
                        from svc in this.selectedApplication.Services 
                        from appService in Cloud.Services 
                            where appService.Name.Equals(svc, StringComparison.InvariantCultureIgnoreCase) 
                        select appService)
                        this.ApplicationServices.Add(appService);                                    
                }
                RaisePropertyChanged("SelectedApplication");
                RaisePropertyChanged("IsApplicationSelected");
            }
        }

        private void SelectedApplicationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplicationErrorMessage = string.Empty;   
            var worker = new BackgroundWorker();
            worker.DoWork += (s, args) =>
            {                
                var result = provider.UpdateApplication(SelectedApplication, Cloud);
                if (!result.Response)
                {
                    dispatcher.BeginInvoke((Action) (() => ApplicationErrorMessage = result.Message));
                    return;
                }
                var appResult = provider.GetApplication(SelectedApplication, Cloud);
                if (appResult.Response == null)
                {
                    dispatcher.BeginInvoke((Action)(() => ApplicationErrorMessage = appResult.Message));
                    return;
                }
                args.Result = appResult.Response;
            };
            worker.RunWorkerCompleted += (s, args) =>
            {
                var result = args.Result as Application;
                if (result != null) 
                    RefreshSelectedApplication(result);
            };
            worker.RunWorkerAsync();
        }

        private void RefreshApplication(object sender, EventArgs e)
        {
            if (null != SelectedApplication)
            {
                var worker = new BackgroundWorker();
                worker.DoWork += (s, ea) => ea.Result = provider.GetApplication(SelectedApplication, Cloud);
                worker.RunWorkerCompleted += (s, ea) =>
                {                    
                    var result = ea.Result as ProviderResponse<Application>;
                    if (result != null && result.Response != null)
                        RefreshSelectedApplication(result.Response);
                };
                worker.RunWorkerAsync();
            }
        }        

        private void RefreshSelectedApplication(Application application)
        {
            SelectedApplication.PropertyChanged -= SelectedApplicationPropertyChanged;
            SelectedApplication.Resources.PropertyChanged -= SelectedApplicationPropertyChanged;
            SelectedApplication.Merge(application);
            SelectedApplication.PropertyChanged += SelectedApplicationPropertyChanged;
            SelectedApplication.Resources.PropertyChanged += SelectedApplicationPropertyChanged;
        }

        #endregion

        #region Commands

        private void ChangePassword()
        {
            this.IsAccountValid = false;
            Messenger.Default.Register<NotificationMessageAction<Types.Cloud>>(this,
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

        private void CreateService()
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
                            (viewModel) => this.SelectedApplication.Uris.Synchronize(viewModel.Urls, StringComparer.InvariantCultureIgnoreCase)));
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
            var worker = new BackgroundWorker();
            worker.DoWork += (s, e) => e.Result = provider.Connect(this.Cloud);
            worker.RunWorkerCompleted += (s,e) =>
            {
                var result = e.Result as ProviderResponse<Cloud>;
                if (result.Response != null)
                    this.Cloud.Merge(result.Response);
                else
                    this.OverviewErrorMessage = result.Message;
            }; 
            worker.RunWorkerAsync();
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
            var worker = new BackgroundWorker();
            worker.DoWork += (s, e) => e.Result = provider.ValidateAccount(this.Cloud);
            worker.RunWorkerCompleted += (s, e) =>
            {
                var result = e.Result as ProviderResponse<bool>;
                if (result.Response)
                    this.IsAccountValid = true;
                else
                    this.OverviewErrorMessage = result.Message;
            };
            worker.RunWorkerAsync();           
        }

        private void Refresh()
        {
            var worker = new BackgroundWorker { WorkerReportsProgress = true };
            SetProgressTitle("Refresh Application");
            worker.ProgressChanged += WorkerProgressChanged;
            worker.DoWork += (s, e) =>
            {
                worker.ReportProgress(30, "Refreshing Application: " + SelectedApplication.Name);
                var appResult = provider.GetApplication(SelectedApplication, Cloud);
                if (appResult.Response == null)
                {
                    worker.ReportProgress(-1, appResult.Message);
                    return;
                }
                e.Result = appResult.Response;
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                var result = e.Result as Application;
                if (result != null)
                    RefreshSelectedApplication(result);
                Messenger.Default.Send(new ProgressMessage(100, "Application Refreshed."));
                applicationStarting = false;
            };
            worker.RunWorkerAsync();
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.Progress, c => { }));
            
        }        

        public void DeleteApplication()
        {
            var worker = new BackgroundWorker {WorkerReportsProgress = true};
            SetProgressTitle("Delete Application");
            worker.ProgressChanged += WorkerProgressChanged;
            worker.DoWork += (s, e) =>
            {
                worker.ReportProgress(30, "Deleting Application: " + SelectedApplication.Name);
                var appResult = provider.Delete(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!appResult.Response)
                {
                    worker.ReportProgress(-1, appResult.Message);
                    return;
                }
                e.Result = appResult.Response;
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                var applicationToRemove = Cloud.Applications.SingleOrDefault((i) => i.Name == SelectedApplication.Name);
                if (applicationToRemove != null)
                {
                    var index = Cloud.Applications.IndexOf(applicationToRemove);
                    Cloud.Applications.RemoveAt(index);
                }
                Messenger.Default.Send(new ProgressMessage(100, "Application Deleted."));
            };
            worker.RunWorkerAsync();
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.Progress, c => { }));
        }

        public void Start()
        {
            applicationStarting = true;
            var worker = new BackgroundWorker {WorkerReportsProgress = true};
            SetProgressTitle("Starting Application");
            worker.ProgressChanged += WorkerProgressChanged;
            worker.DoWork += (s, e) =>
            {
                worker.ReportProgress(25, "Starting Application: " + SelectedApplication.Name);
                var result = provider.Start(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!result.Response)
                {
                    worker.ReportProgress(-1, result.Message);
                    return;
                }
                worker.ReportProgress(75, "Refreshing Application: " + SelectedApplication.Name);
                var appResult = provider.GetApplication(SelectedApplication, Cloud);
                if (appResult.Response == null)
                {
                    worker.ReportProgress(-1, appResult.Message);
                    return;
                }
                e.Result = appResult.Response;
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                var result = e.Result as Application;
                if (result != null)
                    RefreshSelectedApplication(result);
                Messenger.Default.Send(new ProgressMessage(100, "Application Stopped."));
                applicationStarting = false;
            };
            worker.RunWorkerAsync();
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.Progress, c => { }));         
        }
        
        public void Stop()
        {
            var worker = new BackgroundWorker {WorkerReportsProgress = true};
            SetProgressTitle("Stopping Application");
            worker.ProgressChanged += WorkerProgressChanged;
            worker.DoWork += (s, e) => 
            {
                worker.ReportProgress(25,"Stopping Application: " + SelectedApplication.Name);
                var result = provider.Stop(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!result.Response)
                {
                    worker.ReportProgress(-1,result.Message);
                    return;
                }
                worker.ReportProgress(75,"Refreshing Application: " + SelectedApplication.Name);
                var appResult = provider.GetApplication(SelectedApplication, Cloud);
                if (appResult.Response == null)
                {
                    worker.ReportProgress(-1, appResult.Message);
                    return;
                }                
                e.Result = appResult.Response;
            };
            worker.RunWorkerCompleted += (s,e) =>
            {
                var result = e.Result as Application;
                if (result != null)
                    RefreshSelectedApplication(result);
                Messenger.Default.Send(new ProgressMessage(100, "Application Stopped."));                               
            };            
            worker.RunWorkerAsync();
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.Progress, c => { }));
        }

        public void Restart()
        {
            var worker = new BackgroundWorker { WorkerReportsProgress = true };
            SetProgressTitle("Restarting Application");
            worker.ProgressChanged += WorkerProgressChanged;
            worker.DoWork += (s, e) =>
            {
                worker.ReportProgress(25, "Stopping Application: " + SelectedApplication.Name);
                var result = provider.Stop(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!result.Response)
                {
                    worker.ReportProgress(-1, result.Message);
                    return;
                }

                worker.ReportProgress(40, "Refreshing Application: " + SelectedApplication.Name);
                var appResult = provider.GetApplication(SelectedApplication, Cloud);
                if (appResult.Response == null)
                {
                    worker.ReportProgress(-1, appResult.Message);
                    return;
                }
                dispatcher.BeginInvoke((Action)(() => RefreshSelectedApplication(appResult.Response)));

                worker.ReportProgress(60, "Starting Application: " + SelectedApplication.Name);
                result = provider.Start(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!result.Response)
                {
                    worker.ReportProgress(-1, result.Message);
                    return;
                }

                worker.ReportProgress(75, "Refreshing Application: " + SelectedApplication.Name);
                appResult = provider.GetApplication(SelectedApplication, Cloud);
                if (appResult.Response == null)
                {
                    worker.ReportProgress(-1, appResult.Message);
                    return;
                }
                e.Result = appResult.Response;
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                var result = e.Result as Application;
                if (result != null)
                    RefreshSelectedApplication(result);
                Messenger.Default.Send(new ProgressMessage(100, "Application Restarted."));
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

            Messenger.Default.Register<NotificationMessageAction<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.SetProgressCancelButtonVisible))
                        message.Execute(false);
                });
        }

        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var message = e.UserState as string;
            if (e.ProgressPercentage < 0)
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError(message))));
            else
                dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(e.ProgressPercentage, message))));
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
