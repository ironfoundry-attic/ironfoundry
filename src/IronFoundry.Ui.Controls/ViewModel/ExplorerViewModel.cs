namespace IronFoundry.Ui.Controls.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using Cloud;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using Model;
    using Types;
    using Utilities;

    public class ExplorerViewModel : ViewModelBase
    {
        private readonly ObservableCollection<CloudViewModel> clouds = new ObservableCollection<CloudViewModel>();
        private CloudViewModel currentCloudView;
        private string errorMessage;
        private ICloudFoundryProvider provider;
        private CloudViewModel selectedCloudView;

        public ExplorerViewModel()
        {
            CloseCloudCommand = new RelayCommand<CloudViewModel>(CloseCloud);

            Messenger.Default.Send(new NotificationMessageAction<ICloudFoundryProvider>(
                                       Messages.GetCloudFoundryProvider, p => provider = p));

            Messenger.Default.Register<NotificationMessage<Types.Cloud>>(this, ProcessCloudNotification);
            Messenger.Default.Register<NotificationMessage<Application>>(this, ProcessApplicationNotification);
            Messenger.Default.Register<NotificationMessage<string>>(this, ProcessErrorMessage);

            // TODO provider.CloudsChanged += CloudsCollectionChanged;
            provider.CloudRemoved += provider_CloudRemoved;
        }

        public RelayCommand<CloudViewModel> CloseCloudCommand { get; private set; }

        public ObservableCollection<CloudViewModel> Clouds
        {
            get { return clouds; }
        }

        public CloudViewModel CurrentCloudView
        {
            get { return currentCloudView; }
            set
            {
                currentCloudView = value;
                RaisePropertyChanged("CurrentCloudView");
            }
        }

        public CloudViewModel SelectedCloudView
        {
            get { return selectedCloudView; }
            set
            {
                selectedCloudView = value;
                RaisePropertyChanged("SelectedCloudView");
            }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
                if (!String.IsNullOrWhiteSpace(errorMessage))
                {
                    var worker = new BackgroundWorker();
                    worker.DoWork += (s, e) => Thread.Sleep(TimeSpan.FromSeconds(7));
                    worker.RunWorkerCompleted += (s, e) => ErrorMessage = String.Empty;
                    worker.RunWorkerAsync();
                }
            }
        }

        private void provider_CloudRemoved(object sender, CloudEventArgs e)
        {
            Types.Cloud cloud = e.Cloud;
            CloudViewModel cloudViewItem = clouds.SingleOrDefault((i) => i.Cloud.Equals(cloud));
            clouds.Remove(cloudViewItem);
        }

        private void CloseCloud(CloudViewModel cloudView)
        {
            Messenger.Default.Unregister(cloudView);
            Clouds.Remove(cloudView);
            provider.CloudRemoved -= provider_CloudRemoved;
        }

        private void OpenApplication(Application application)
        {
            if (application.Parent != null)
            {
                CloudViewModel selectedCloudViewModel = Clouds.SingleOrDefault((i) => i.Cloud.Equals(application.Parent));
                if (selectedCloudViewModel == null)
                {
                    Types.Cloud currentCloud = provider.Clouds.SingleOrDefault((c) => c.Equals(application.Parent));
                    selectedCloudViewModel = new CloudViewModel(currentCloud);
                    Clouds.Add(selectedCloudViewModel);
                }
                SelectedCloudView = selectedCloudViewModel;
            }
            SelectedCloudView.SelectedApplication = application;
            SelectedCloudView.IsApplicationViewSelected = true;
        }

        private void ProcessErrorMessage(NotificationMessage<string> message)
        {
            if (message.Notification.Equals(Messages.ErrorMessage))
                ErrorMessage = message.Content;
        }

        private void ProcessCloudNotification(NotificationMessage<Types.Cloud> message)
        {
            if (message.Notification.Equals(Messages.OpenCloud))
            {
                CloudViewModel selectedCloudViewModel = Clouds.SingleOrDefault((i) => i.Cloud.Equals(message.Content));
                if (selectedCloudViewModel == null)
                {
                    selectedCloudViewModel = new CloudViewModel(message.Content);
                    Clouds.Add(selectedCloudViewModel);
                }
                SelectedCloudView = selectedCloudViewModel;
            }
        }

        private void ProcessApplicationNotification(NotificationMessage<Application> message)
        {
            OpenApplication(message.Content);
            switch (message.Notification)
            {
                case Messages.StartApplication:
                    SelectedCloudView.Start();
                    break;
                case Messages.StopApplication:
                    SelectedCloudView.Stop();
                    break;
                case Messages.RestartApplication:
                    SelectedCloudView.Restart();
                    break;
                case Messages.DeleteApplication:
                    SelectedCloudView.DeleteApplication();
                    break;
            }
        }
    }
}
