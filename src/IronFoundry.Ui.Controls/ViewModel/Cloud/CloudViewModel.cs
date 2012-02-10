namespace IronFoundry.Ui.Controls.ViewModel.Cloud
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using IronFoundry.Utilities;
    using Model;
    using Mvvm;
    using Types;
    using Utilities;
    using Application = Types.Application;

    public class CloudViewModel : ViewModelBase
    {
        private readonly Dispatcher dispatcher;
        private string applicationErrorMessage;
        private SafeObservableCollection<ProvisionedService> applicationServices;
        private IDropTarget applicationServicesTarget;
        private bool applicationStarting;
        private Cloud cloud;
        private bool isAccountValid;
        private bool isApplicationViewSelected;
        private string overviewErrorMessage;
        private ICloudFoundryProvider provider;
        private IDragSource provisionedServicesSource;
        private Application selectedApplication;
        private ProvisionedService selectedApplicationService;

        public CloudViewModel(Cloud cloud)
        {
            Messenger.Default.Send(new NotificationMessageAction<ICloudFoundryProvider>(
                                       Messages.GetCloudFoundryProvider, p => provider = p));
            dispatcher = Dispatcher.CurrentDispatcher;
            Cloud = cloud;

            ChangePasswordCommand           = new RelayCommand(ChangePassword);
            ValidateAccountCommand          = new RelayCommand(ValidateAccount, CanExecuteValidateAccount);
            ConnectCommand                  = new RelayCommand(Connect, CanExecuteConnect);
            DisconnectCommand               = new RelayCommand(Disconnect, CanExecuteDisconnect);
            StartCommand                    = new RelayCommand(Start, CanExecuteStart);
            StopCommand                     = new RelayCommand(Stop, CanExecuteStopActions);
            RestartCommand                  = new RelayCommand(Restart, CanExecuteStopActions);
            ManageApplicationUrlsCommand    = new RelayCommand(ManageApplicationUrls);
            RemoveApplicationServiceCommand = new RelayCommand(RemoveApplicationService);
            ProvisionServiceCommand         = new RelayCommand(CreateService);
            RefreshCommand                  = new RelayCommand(Refresh, CanExecuteRefresh);
            DeleteApplicationCommand        = new RelayCommand(DeleteApplication);
        }

        #region Properties

        public string OverviewErrorMessage
        {
            get { return overviewErrorMessage; }
            set
            {
                overviewErrorMessage = value;
                RaisePropertyChanged("OverviewErrorMessage");
                if (!String.IsNullOrWhiteSpace(overviewErrorMessage))
                {
                    var worker = new BackgroundWorker();
                    worker.DoWork += (s, e) => Thread.Sleep(TimeSpan.FromSeconds(7));
                    worker.RunWorkerCompleted += (s, e) => OverviewErrorMessage = string.Empty;
                    worker.RunWorkerAsync();
                }
            }
        }

        public string ApplicationErrorMessage
        {
            get { return applicationErrorMessage; }
            set
            {
                applicationErrorMessage = value;
                RaisePropertyChanged("ApplicationErrorMessage");
                if (!String.IsNullOrWhiteSpace(applicationErrorMessage))
                {
                    var worker = new BackgroundWorker();
                    worker.DoWork += (s, e) => Thread.Sleep(TimeSpan.FromSeconds(7));
                    worker.RunWorkerCompleted += (s, e) => ApplicationErrorMessage = string.Empty;
                    worker.RunWorkerAsync();
                }
            }
        }

        public bool IsAccountValid
        {
            get { return isAccountValid; }
            set
            {
                isAccountValid = value;
                RaisePropertyChanged("IsAccountValid");
            }
        }

        public Cloud Cloud
        {
            get { return cloud; }
            set
            {
                cloud = value;
                RaisePropertyChanged("Cloud");
            }
        }

        public ProvisionedService SelectedApplicationService
        {
            get { return selectedApplicationService; }
            set
            {
                selectedApplicationService = value;
                RaisePropertyChanged("SelectedApplicationService");
            }
        }

        public int[] MemoryLimits
        {
            get { return Constants.MemoryLimits; }
        }

        public SafeObservableCollection<Application> Applications
        {
            get { return Cloud.Applications; }
        }

        public bool IsApplicationSelected
        {
            get { return SelectedApplication != null; }
        }

        public bool IsNotApplicationViewSelected
        {
            get { return !isApplicationViewSelected; }
        }

        public bool IsApplicationViewSelected
        {
            get { return isApplicationViewSelected; }
            set
            {
                isApplicationViewSelected = value;
                RaisePropertyChanged("IsApplicationViewSelected");
            }
        }

        public SafeObservableCollection<ProvisionedService> ApplicationServices
        {
            get { return applicationServices; }
            set
            {
                applicationServices = value;
                RaisePropertyChanged("ApplicationServices");
            }
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
            return IsApplicationSelected && false == applicationStarting && SelectedApplication.CanStart;
        }

        public bool CanExecuteStopActions()
        {
            return IsApplicationSelected && SelectedApplication.CanStop;
        }

        private bool CanExecuteRefresh()
        {
            return SelectedApplication != null;
        }

        #endregion

        #region Selected Application

        public Application SelectedApplication
        {
            get { return selectedApplication; }
            set
            {
                if (SelectedApplication != null)
                {
                    SelectedApplication.PropertyChanged -= SelectedApplicationPropertyChanged;
                    SelectedApplication.Resources.PropertyChanged -= SelectedApplicationPropertyChanged;
                }
                selectedApplication = value;
                if (selectedApplication != null)
                {
                    selectedApplication.PropertyChanged += SelectedApplicationPropertyChanged;
                    selectedApplication.Resources.PropertyChanged += SelectedApplicationPropertyChanged;

                    ApplicationServices = new SafeObservableCollection<ProvisionedService>();
                    foreach (ProvisionedService appService in 
                        from svc in selectedApplication.Services
                        from appService in Cloud.Services
                        where appService.Name.Equals(svc, StringComparison.InvariantCultureIgnoreCase)
                        select appService)
                        ApplicationServices.Add(appService);
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
                ProviderResponse<bool> result = provider.UpdateApplication(SelectedApplication, Cloud);
                if (!result.Response)
                {
                    dispatcher.BeginInvoke((Action) (() => ApplicationErrorMessage = result.Message));
                    return;
                }
                ProviderResponse<Application> appResult = provider.GetApplication(SelectedApplication, Cloud);
                if (appResult.Response == null)
                {
                    dispatcher.BeginInvoke((Action) (() => ApplicationErrorMessage = appResult.Message));
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
            IsAccountValid = false;
            Messenger.Default.Register<NotificationMessageAction<Cloud>>(this,
                                                                         message =>
                                                                         {
                                                                             if (
                                                                                 message.Notification.Equals(
                                                                                     Messages.SetChangePasswordData))
                                                                                 message.Execute(Cloud);
                                                                         });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ChangePassword,
                                                                       (confirmed) =>
                                                                       {
                                                                           if (confirmed)
                                                                               Messenger.Default.Send(
                                                                                   new NotificationMessageAction
                                                                                       <ChangePasswordViewModel>(
                                                                                       Messages.GetChangePasswordData,
                                                                                       v =>
                                                                                       Cloud.Password = v.NewPassword));
                                                                       }));
        }

        private void CreateService()
        {
            Messenger.Default.Register<NotificationMessageAction<Cloud>>(this,
                                                                         message =>
                                                                         {
                                                                             if (
                                                                                 message.Notification.Equals(
                                                                                     Messages.SetCreateServiceData))
                                                                                 message.Execute(Cloud);
                                                                         });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.CreateService,
                                                                       (confirmed) =>
                                                                       {
                                                                           if (confirmed)
                                                                           {
                                                                               Messenger.Default.Send(
                                                                                   new NotificationMessageAction
                                                                                       <CreateServiceViewModel>(
                                                                                       Messages.GetCreateServiceData,
                                                                                       (viewModel) =>
                                                                                       {
                                                                                           ProviderResponse
                                                                                               <
                                                                                                   SafeObservableCollection
                                                                                                       <
                                                                                                           ProvisionedService
                                                                                                           >> result =
                                                                                                               provider.
                                                                                                                   GetProvisionedServices
                                                                                                                   (Cloud);
                                                                                           if (result.Response == null)
                                                                                               ApplicationErrorMessage =
                                                                                                   result.Message;
                                                                                           else
                                                                                               Cloud.Services.
                                                                                                   Synchronize(
                                                                                                       result.Response,
                                                                                                       new ProvisionedServiceEqualityComparer
                                                                                                           ());
                                                                                       }));
                                                                           }
                                                                       }));
        }

        private void ManageApplicationUrls()
        {
            Messenger.Default.Register<NotificationMessageAction<SafeObservableCollection<string>>>(this,
                                                                                                    message =>
                                                                                                    {
                                                                                                        if (
                                                                                                            message.
                                                                                                                Notification
                                                                                                                .Equals(
                                                                                                                    Messages
                                                                                                                        .
                                                                                                                        SetManageApplicationUrlsData))
                                                                                                            message.
                                                                                                                Execute(
                                                                                                                    SelectedApplication
                                                                                                                        .
                                                                                                                        Uris
                                                                                                                        .
                                                                                                                        DeepCopy
                                                                                                                        ());
                                                                                                    });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ManageApplicationUrls,
                                                                       (confirmed) =>
                                                                       {
                                                                           if (confirmed)
                                                                           {
                                                                               Messenger.Default.Send(
                                                                                   new NotificationMessageAction
                                                                                       <ManageApplicationUrlsViewModel>(
                                                                                       Messages.
                                                                                           GetManageApplicationUrlsData,
                                                                                       (viewModel) =>
                                                                                       SelectedApplication.Uris.
                                                                                           Synchronize(viewModel.Urls,
                                                                                                       StringComparer.
                                                                                                           InvariantCultureIgnoreCase)));
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
                ApplicationServices.Remove(SelectedApplicationService);
            }
        }

        private void Connect()
        {
            OverviewErrorMessage = string.Empty;
            var worker = new BackgroundWorker();
            worker.DoWork += (s, e) => e.Result = provider.Connect(Cloud);
            worker.RunWorkerCompleted += (s, e) =>
            {
                var result = e.Result as ProviderResponse<Cloud>;
                if (result.Response != null)
                    Cloud.Merge(result.Response);
                else
                    OverviewErrorMessage = result.Message;
            };
            worker.RunWorkerAsync();
        }

        private void Disconnect()
        {
            Cloud = provider.Disconnect(Cloud);
            IsAccountValid = false;
        }

        private void ValidateAccount()
        {
            OverviewErrorMessage = string.Empty;
            IsAccountValid = false;
            var worker = new BackgroundWorker();
            worker.DoWork += (s, e) => e.Result = provider.ValidateAccount(Cloud);
            worker.RunWorkerCompleted += (s, e) =>
            {
                var result = e.Result as ProviderResponse<bool>;
                if (result.Response)
                {
                    IsAccountValid = true;
                }
                else
                {
                    OverviewErrorMessage = result.Message;
                }
            };
            worker.RunWorkerAsync();
        }

        private void Refresh()
        {
            var worker = new BackgroundWorker {WorkerReportsProgress = true};
            SetProgressTitle("Refresh Application");
            worker.ProgressChanged += WorkerProgressChanged;
            worker.DoWork += (s, e) =>
            {
                worker.ReportProgress(30, "Refreshing Application: " + SelectedApplication.Name);
                ProviderResponse<Application> appResult = provider.GetApplication(SelectedApplication, Cloud);
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
                ProviderResponse<bool> appResult = provider.Delete(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!appResult.Response)
                {
                    worker.ReportProgress(-1, appResult.Message);
                    return;
                }
                e.Result = appResult.Response;
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                Application applicationToRemove =
                    Cloud.Applications.SingleOrDefault((i) => i.Name == SelectedApplication.Name);
                if (applicationToRemove != null)
                {
                    int index = Cloud.Applications.IndexOf(applicationToRemove);
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
                ProviderResponse<bool> result = provider.Start(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!result.Response)
                {
                    worker.ReportProgress(-1, result.Message);
                    return;
                }
                worker.ReportProgress(75, "Refreshing Application: " + SelectedApplication.Name);
                ProviderResponse<Application> appResult = provider.GetApplication(SelectedApplication, Cloud);
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
                Messenger.Default.Send(new ProgressMessage(100, "Application Started."));
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
                worker.ReportProgress(25, "Stopping Application: " + SelectedApplication.Name);
                ProviderResponse<bool> result = provider.Stop(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!result.Response)
                {
                    worker.ReportProgress(-1, result.Message);
                    return;
                }
                worker.ReportProgress(75, "Refreshing Application: " + SelectedApplication.Name);
                ProviderResponse<Application> appResult = provider.GetApplication(SelectedApplication, Cloud);
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
            };
            worker.RunWorkerAsync();
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.Progress, c => { }));
        }

        public void Restart()
        {
            var worker = new BackgroundWorker {WorkerReportsProgress = true};
            SetProgressTitle("Restarting Application");
            worker.ProgressChanged += WorkerProgressChanged;
            worker.DoWork += (s, e) =>
            {
                worker.ReportProgress(25, "Stopping Application: " + SelectedApplication.Name);
                ProviderResponse<bool> result = provider.Stop(SelectedApplication.DeepCopy(), Cloud.DeepCopy());
                if (!result.Response)
                {
                    worker.ReportProgress(-1, result.Message);
                    return;
                }

                worker.ReportProgress(40, "Refreshing Application: " + SelectedApplication.Name);
                ProviderResponse<Application> appResult = provider.GetApplication(SelectedApplication, Cloud);
                if (appResult.Response == null)
                {
                    worker.ReportProgress(-1, appResult.Message);
                    return;
                }
                dispatcher.BeginInvoke((Action) (() => RefreshSelectedApplication(appResult.Response)));

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
                                                                              if (
                                                                                  message.Notification.Equals(
                                                                                      Messages.SetProgressData))
                                                                                  message.Execute(title);
                                                                          });

            Messenger.Default.Register<NotificationMessageAction<bool>>(this,
                                                                        message =>
                                                                        {
                                                                            if (
                                                                                message.Notification.Equals(
                                                                                    Messages.
                                                                                        SetProgressCancelButtonVisible))
                                                                                message.Execute(false);
                                                                        });
        }

        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var message = e.UserState as string;
            if (e.ProgressPercentage < 0)
                dispatcher.BeginInvoke((Action) (() => Messenger.Default.Send(new ProgressError(message))));
            else
                dispatcher.BeginInvoke(
                    (Action) (() => Messenger.Default.Send(new ProgressMessage(e.ProgressPercentage, message))));
        }

        #endregion

        #region DragDropProvisionedServices

        public IDragSource SourceOfProvisionedServices
        {
            get
            {
                if (provisionedServicesSource == null)
                    provisionedServicesSource = new DragSource<ProvisionedService>(
                        GetBindProvisionedServicesDragEffects, GetBindProvisionedServicesData);
                return provisionedServicesSource;
            }
        }

        public IDropTarget ApplicationServiceSink
        {
            get
            {
                if (applicationServicesTarget == null)
                    applicationServicesTarget = new DropTarget<ProvisionedService>(GetApplicationServicesDropEffects,
                                                                                   DropApplicationServices);
                return applicationServicesTarget;
            }
        }

        private DragDropEffects GetBindProvisionedServicesDragEffects(ProvisionedService provisionedService)
        {
            return Cloud.Services.Any() ? DragDropEffects.Move : DragDropEffects.None;
        }

        private object GetBindProvisionedServicesData(ProvisionedService provisionedService)
        {
            return provisionedService;
        }

        private void DropApplicationServices(ProvisionedService provisionedService)
        {
            ApplicationServices.Add(provisionedService);
            foreach (ProvisionedService service in ApplicationServices)
            {
                if (!SelectedApplication.Services.Contains(service.Name))
                {
                    SelectedApplication.Services.Add(service.Name);
                    RaisePropertyChanged("ApplicationServices");
                }
            }
        }

        private DragDropEffects GetApplicationServicesDropEffects(ProvisionedService provisionedService)
        {
            ProvisionedService existingService =
                ApplicationServices.SingleOrDefault(
                    i => i.Name.Equals(provisionedService.Name, StringComparison.InvariantCultureIgnoreCase));
            return (existingService != null) ? DragDropEffects.None : DragDropEffects.Move;
        }

        #endregion

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
    }
}