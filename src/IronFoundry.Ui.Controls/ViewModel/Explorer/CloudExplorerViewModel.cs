using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using IronFoundry.Types;
using IronFoundry.Ui.Controls.Model;
using IronFoundry.Ui.Controls.Mvvm;
using IronFoundry.Ui.Controls.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using IronFoundry.Ui.Controls.ViewModel.Push;

namespace IronFoundry.Ui.Controls.ViewModel.Explorer
{
    using Model;
    using Mvvm;
    using Push;
    using Utilities;

    public class CloudExplorerViewModel : ViewModelBase
    {
        private ICloudFoundryProvider provider;
        private Dispatcher dispatcher;
        private ObservableCollection<CloudTreeViewItemViewModel> clouds = new ObservableCollection<CloudTreeViewItemViewModel>();
        public RelayCommand AddCloudCommand { get; set; }
        public RelayCommand PushAppCommand { get; set; }
        public RelayCommand UpdateAppCommand { get; set; }
        public RelayCommand RefreshCloudsCommand { get; set; }

        public CloudExplorerViewModel()
        {
            Messenger.Default.Send(new NotificationMessageAction<ICloudFoundryProvider>(Messages.GetCloudFoundryProvider, (p) => this.provider = p));
            this.dispatcher = Dispatcher.CurrentDispatcher;
            AddCloudCommand = new RelayCommand(AddCloud);
            PushAppCommand = new RelayCommand(PushApp);
            UpdateAppCommand = new RelayCommand(UpdateApp);
            foreach (var cloud in provider.Clouds)
                clouds.Add(new CloudTreeViewItemViewModel(cloud));
            this.provider.CloudsChanged += CloudsCollectionChanged;
        }

        private void CloudsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action.Equals(NotifyCollectionChangedAction.Add))
            {
                foreach (var obj in e.NewItems)
                {
                    var cloud = obj as Types.Cloud;
                    clouds.Add(new CloudTreeViewItemViewModel(cloud));
                }
            }
            else if (e.Action.Equals(NotifyCollectionChangedAction.Remove))
            {
                foreach (var obj in e.OldItems)
                {
                    var cloud = obj as Types.Cloud;
                    var cloudTreeViewItem = clouds.SingleOrDefault((i) => i.Cloud.Equals(cloud));
                    clouds.Remove(cloudTreeViewItem);
                }
            }
        }

        public ObservableCollection<CloudTreeViewItemViewModel> Clouds
        {
            get { return this.clouds; }
        }

        private void AddCloud()
        {
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.AddCloud, (confirmed) => { }));
        }

        private void PushApp()
        {
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.PushApp,
            (confirmed) =>
            {
                if (confirmed)
                {
                    PushViewModel viewModel = null;
                    Messenger.Default.Send(new NotificationMessageAction<PushViewModel>(Messages.GetPushAppData, vm => viewModel = vm));


                    var worker = new BackgroundWorker {WorkerReportsProgress = true};
                    SetProgressTitle("Push Application");
                    worker.ProgressChanged += WorkerProgressChanged;
                    worker.DoWork += (s, e) =>
                    {
                        worker.ReportProgress(10, "Pushing Application: " + viewModel.Name);

                        var result = provider.Push(viewModel.SelectedCloud,
                                                   viewModel.Name,
                                                   viewModel.Url,
                                                   Convert.ToUInt16(viewModel.Instances),
                                                   viewModel.PushFromDirectory,
                                                   Convert.ToUInt32(viewModel.SelectedMemory),
                                                   viewModel.ApplicationServices.Select(provisionedService => provisionedService.Name).ToArray());

                        if (!result.Response)
                        {
                            worker.ReportProgress(-1, result.Message);
                            return;
                        }

                        var appResult = provider.GetApplication(new Application() { Name = viewModel.Name }, viewModel.SelectedCloud);
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
                        var cloud = provider.Clouds.SingleOrDefault((c) => c.ID == viewModel.SelectedCloud.ID);
                        if (result != null) 
                            cloud.Applications.Add(result);
                        Messenger.Default.Send(new ProgressMessage(100, "Application Pushed."));
                    };
                    worker.RunWorkerAsync();
                    Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.Progress, c => { }));
                }

            }));
        }

        private void UpdateApp()
        {
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.UpdateApp,
            (confirmed) =>
            {
                if (confirmed)
                {
                    UpdateViewModel viewModel = null;
                    Messenger.Default.Send(new NotificationMessageAction<UpdateViewModel>(Messages.GetUpdateAppData, vm => viewModel = vm));

                    var worker = new BackgroundWorker { WorkerReportsProgress = true };
                    SetProgressTitle("Update Application");
                    worker.ProgressChanged += WorkerProgressChanged;
                    worker.DoWork += (s, e) =>
                    {
                        worker.ReportProgress(10, "Updating Application: " + viewModel.SelectedApplication.Name);                        
                        var result = provider.Update(viewModel.SelectedCloud, 
                                                     viewModel.SelectedApplication,
                                                     viewModel.PushFromDirectory);
                        if (!result.Response)
                        {
                            worker.ReportProgress(-1, result.Message);
                            return;
                        }

                        worker.ReportProgress(75, "Refreshing Application: " + viewModel.SelectedApplication.Name);
                        var appResult = provider.GetApplication(viewModel.SelectedApplication, viewModel.SelectedCloud);
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
                        var cloud = provider.Clouds.SingleOrDefault((c) => c.ID == viewModel.SelectedCloud.ID);
                        if (cloud != null)
                        {
                            var application = cloud.Applications.SingleOrDefault((i) => i.Name == result.Name);
                            if (application != null)
                                application.Merge(result);
                        }
                        Messenger.Default.Send(new ProgressMessage(100, "Application Updated."));
                    };
                    worker.RunWorkerAsync();
                    Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.Progress, c => { }));
                }
            }));
        }

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

    }
}