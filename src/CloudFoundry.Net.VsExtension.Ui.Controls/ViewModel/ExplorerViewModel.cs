using System.Linq;
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

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    [ExportViewModel("Explorer", true)]
    public class ExplorerViewModel : ViewModelBase
    {
        private CloudFoundryProvider provider;
        private CloudViewModel currentCloudView;
        private readonly ObservableCollection<CloudViewModel> clouds = new ObservableCollection<CloudViewModel>();
        private CloudViewModel selectedCloudView;
        
        public RelayCommand<CloudViewModel> CloseCloudCommand { get; private set; }

        public ExplorerViewModel()
        {
            CloseCloudCommand = new RelayCommand<CloudViewModel>(CloseCloud);
            Messenger.Default.Send<NotificationMessageAction<CloudFoundryProvider>>(new NotificationMessageAction<CloudFoundryProvider>(Messages.GetCloudFoundryProvider,LoadProvider));
            Messenger.Default.Register<NotificationMessage<Cloud>>(this, ProcessCloudNotification);
            Messenger.Default.Register<NotificationMessage<Application>>(this, ProcessApplicationNotification);
        }

        private void LoadProvider(CloudFoundryProvider provider)
        {
            this.provider = provider;
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
            if (message.Notification.Equals(Messages.StartApplication))
            {
                this.SelectedCloudView.Start();
            }
            else if (message.Notification.Equals(Messages.StopApplication))
            {
                this.SelectedCloudView.Stop();
            }
            else if (message.Notification.Equals(Messages.RestartApplication))
            {
                this.SelectedCloudView.Restart();
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
    }
}
