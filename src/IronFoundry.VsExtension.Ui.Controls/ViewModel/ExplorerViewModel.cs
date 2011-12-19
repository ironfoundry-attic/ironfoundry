namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using CloudFoundry.Net.Types;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;

    public class ExplorerViewModel : ViewModelBase
    {
        private CloudViewModel currentCloudView;
        private readonly ObservableCollection<CloudViewModel> clouds = new ObservableCollection<CloudViewModel>();
        private CloudViewModel selectedCloudView;
        private ICloudFoundryProvider provider;
        private string errorMessage;
        
        public RelayCommand<CloudViewModel> CloseCloudCommand { get; private set; }

        public ExplorerViewModel()
        {
            CloseCloudCommand = new RelayCommand<CloudViewModel>(CloseCloud);
            Messenger.Default.Send(new NotificationMessageAction<ICloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            Messenger.Default.Register<NotificationMessage<Cloud>>(this, ProcessCloudNotification);
            Messenger.Default.Register<NotificationMessage<Application>>(this, ProcessApplicationNotification);
            Messenger.Default.Register<NotificationMessage<string>>(this, ProcessErrorMessage);
            this.provider.CloudsChanged += CloudsCollectionChanged;
        }

        private void CloudsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action.Equals(NotifyCollectionChangedAction.Remove))
            {
                foreach (var obj in e.OldItems)
                {
                    var cloud = obj as Cloud;
                    var cloudViewItem = clouds.SingleOrDefault((i) => i.Cloud.Equals(cloud));
                    clouds.Remove(cloudViewItem);
                }
            }
        }  

        private void CloseCloud(CloudViewModel cloudView)
        {
            Messenger.Default.Unregister(cloudView);
            this.Clouds.Remove(cloudView);
        }

        private void OpenApplication(Application application)
        {
            if (application.Parent != null)
            {
                var selectedCloudViewModel = this.Clouds.SingleOrDefault((i) => i.Cloud.Equals(application.Parent));
                if (selectedCloudViewModel == null)
                {
                    var currentCloud = provider.Clouds.SingleOrDefault((c) => c.Equals(application.Parent));
                    selectedCloudViewModel = new CloudViewModel(currentCloud);
                    this.Clouds.Add(selectedCloudViewModel);
                }
                this.SelectedCloudView = selectedCloudViewModel;
            }                
            this.SelectedCloudView.SelectedApplication = application;
            this.SelectedCloudView.IsApplicationViewSelected = true;
        }

        private void ProcessErrorMessage(NotificationMessage<string> message)
        {
            if (message.Notification.Equals(Messages.ErrorMessage))
                this.ErrorMessage = message.Content;
        }

        private void ProcessCloudNotification(NotificationMessage<Cloud> message)
        {
            if (message.Notification.Equals(Messages.OpenCloud))
            {
                var selectedCloudViewModel = this.Clouds.SingleOrDefault((i) => i.Cloud.Equals(message.Content));
                if (selectedCloudViewModel == null)
                {
                    selectedCloudViewModel = new CloudViewModel(message.Content);
                    this.Clouds.Add(selectedCloudViewModel);
                }
                this.SelectedCloudView = selectedCloudViewModel;
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

        public ObservableCollection<CloudViewModel> Clouds
        {
            get { return this.clouds; }
        }        

        public CloudViewModel CurrentCloudView
        {
            get { return this.currentCloudView; }
            set { this.currentCloudView = value; RaisePropertyChanged("CurrentCloudView"); }
        }

        public CloudViewModel SelectedCloudView
        {
            get { return this.selectedCloudView; }
            set { this.selectedCloudView = value; RaisePropertyChanged("SelectedCloudView"); }
        }

        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set { this.errorMessage = value; RaisePropertyChanged("ErrorMessage");
                if (!String.IsNullOrWhiteSpace(this.errorMessage))
                {
                    var worker = new BackgroundWorker();
                    worker.DoWork += (s, e) => Thread.Sleep(TimeSpan.FromSeconds(7));
                    worker.RunWorkerCompleted += (s, e) => this.ErrorMessage = string.Empty;
                    worker.RunWorkerAsync();
                }
            }
        }
    }
}