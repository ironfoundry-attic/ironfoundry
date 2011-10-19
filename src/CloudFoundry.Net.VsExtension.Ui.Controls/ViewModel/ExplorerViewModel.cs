using System.Linq;
using CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel;
using GalaSoft.MvvmLight;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using System.Collections.ObjectModel;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using GalaSoft.MvvmLight.Command;
using CloudFoundry.Net.Vmc;
using CloudFoundry.Net.Types;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Specialized;
using System;
using System.Threading;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class ExplorerViewModel : ViewModelBase
    {
        private CloudViewModel currentCloudView;
        private readonly ObservableCollection<CloudViewModel> clouds = new ObservableCollection<CloudViewModel>();
        private CloudViewModel selectedCloudView;
        private CloudFoundryProvider provider;
        private string errorMessage;
        
        public RelayCommand<CloudViewModel> CloseCloudCommand { get; private set; }

        public ExplorerViewModel()
        {
            CloseCloudCommand = new RelayCommand<CloudViewModel>(CloseCloud);
            Messenger.Default.Send<NotificationMessageAction<CloudFoundryProvider>>(new NotificationMessageAction<CloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
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
                    selectedCloudViewModel = new CloudViewModel(application.Parent);
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
