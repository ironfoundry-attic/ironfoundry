using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using GalaSoft.MvvmLight;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.Types;
using System.ComponentModel;
using CloudFoundry.Net.Vmc;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class CloudExplorerViewModel : ViewModelBase
    {
        private ICloudFoundryProvider provider;
        private Dispatcher dispatcher;
        private ObservableCollection<CloudTreeViewItemViewModel> clouds = new ObservableCollection<CloudTreeViewItemViewModel>();
        public RelayCommand AddCloudCommand { get; private set; }
        public RelayCommand PushAppCommand { get; private set; }
        public RelayCommand UpdateAppCommand { get; private set; }
        public RelayCommand RefreshCloudsCommand { get; private set; }

        public CloudExplorerViewModel()
        {
            Messenger.Default.Send<NotificationMessageAction<ICloudFoundryProvider>>(new NotificationMessageAction<ICloudFoundryProvider>(Messages.GetCloudFoundryProvider, (p) => this.provider = p));
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
                    var cloud = obj as Cloud;
                    clouds.Add(new CloudTreeViewItemViewModel(cloud));
                }
            }
            else if (e.Action.Equals(NotifyCollectionChangedAction.Remove))
            {
                foreach (var obj in e.OldItems)
                {
                    var cloud = obj as Cloud;
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
                    PushViewModel pushViewModel = null;
                    Messenger.Default.Send(new NotificationMessageAction<PushViewModel>(Messages.GetPushAppData, vm => pushViewModel = vm));

                    var worker = new BackgroundWorker();
                    SetProgressTitle("Push Application");
                    worker.DoWork += (s, args) =>
                    {
                        dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(10, "Pushing Application..."))));
                        List<string> services = new List<string>();
                        foreach (var provisionedService in pushViewModel.ApplicationServices)
                            services.Add(provisionedService.Name);

                        var result = provider.Push(pushViewModel.SelectedCloud,
                                                   pushViewModel.Name,
                                                   pushViewModel.Url,
                                                   Convert.ToUInt16(pushViewModel.Instances),
                                                   pushViewModel.PushFromDirectory,
                                                   Convert.ToUInt32(pushViewModel.SelectedMemory),
                                                   services.ToArray());

                        if (!result.Response)
                        {
                            dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError("Push Error: " + result.Message))));
                            return;
                        }

                        var refreshResult = provider.GetApplication(new Application() { Name = pushViewModel.Name }, pushViewModel.SelectedCloud);
                        if (refreshResult.Response == null)
                        {
                            dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError("Push Error: " + result.Message))));
                            return;
                        }

                        dispatcher.BeginInvoke((Action)(() =>
                        {
                            Messenger.Default.Send(new ProgressMessage(100, "Application Pushed."));
                            var cloud = provider.Clouds.SingleOrDefault((c) => c.ID == pushViewModel.SelectedCloud.ID);
                            cloud.Applications.Add(refreshResult.Response);
                        }));

                        dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(100, "Application Pushed."))));
                    };
                    worker.RunWorkerAsync();
                    Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ExplorerProgress, c => { }));
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
                    UpdateViewModel updateViewModel = null;
                    Messenger.Default.Send(new NotificationMessageAction<UpdateViewModel>(Messages.GetUpdateAppData, vm => updateViewModel = vm));

                    var worker = new BackgroundWorker();
                    SetProgressTitle("Update Application");
                    worker.DoWork += (s, args) =>
                    {
                        dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(10, "Updating Application..."))));

                        var result = provider.Update(updateViewModel.SelectedCloud, updateViewModel.SelectedApplication,
                                                    updateViewModel.PushFromDirectory);
                        if (!result.Response)
                        {
                            dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError("Update Error: " + result.Message))));
                            return;
                        }

                        dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(75, "Refreshing Application..."))));
                        var refreshResult = provider.GetApplication(new Application() { Name = updateViewModel.SelectedApplication.Name }, updateViewModel.SelectedCloud);
                        if (refreshResult.Response == null)
                        {
                            dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressError("Update Error: " + result.Message))));
                            return;
                        }

                        var cloud = provider.Clouds.SingleOrDefault((c) => c.ID == updateViewModel.SelectedCloud.ID);
                        var applicationToReplace = cloud.Applications.SingleOrDefault((i) => i.Name == refreshResult.Response.Name);
                        dispatcher.BeginInvoke((Action)(() =>
                        {                            
                            if (applicationToReplace != null)
                            {
                                var index = cloud.Applications.IndexOf(applicationToReplace);
                                cloud.Applications[index] = refreshResult.Response;
                            }
                        }));

                        dispatcher.BeginInvoke((Action)(() => Messenger.Default.Send(new ProgressMessage(100, "Application Updated."))));
                    };
                    worker.RunWorkerAsync();
                    Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ExplorerProgress, c => { }));
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
        }

        #endregion

    }
}
