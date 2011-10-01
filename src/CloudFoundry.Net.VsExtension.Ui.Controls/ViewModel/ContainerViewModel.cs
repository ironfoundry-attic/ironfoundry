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
    [ExportViewModel("Container", true)]
    public class ContainerViewModel : ViewModelBase
    {
        private CloudExplorerViewModel cloudExplorer;
        private CloudViewModel currentCloudView;
        private ObservableCollection<CloudUrl> cloudUrls;
        private ObservableCollection<CloudViewModel> clouds;
        private CloudViewModel selectedCloudView;
        private const string preferencesFileName = "preferences.bin";
        public RelayCommand<CloudViewModel> CloseCloud { get; private set; }

        public ContainerViewModel()
        {
            LoadPreferences();
            CloseCloud = new RelayCommand<CloudViewModel>(RemoveCloud);
            Clouds = new ObservableCollection<CloudViewModel>();

            Messenger.Default.Register<NotificationMessage<ObservableCollection<Cloud>>>(this, ProcessCloudListNotification);
            Messenger.Default.Register<NotificationMessage<Cloud>>(this, ProcessCloudNotification);
            Messenger.Default.Register<NotificationMessage<Application>>(this, ProcessApplicationNotification);
            Messenger.Default.Register<NotificationMessageAction<ObservableCollection<CloudUrl>>>(this, ProcessCloudUrlsNotification);
        }

        private void CloudUrlsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SavePreferences();
        }

        private void CloudChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "AccessToken":
                case "Email":
                case "ServerName":
                case "HostName":
                case "Url":
                case "TimeoutStart":
                case "TimeoutStop":
                case "ID":
                case "Password":
                case "IsConnected":
                case "IsDisconnected":
                    SavePreferences();
                    break;
                default:
                    break;
            }
        }

        public void CloudsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SavePreferences();
        }

        private void LoadPreferences()
        {
            var clouds = new ObservableCollection<Cloud>();
            this.cloudUrls = CloudUrl.GetDefaultCloudUrls();

            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.FileExists(preferencesFileName))
            {
                using (IsolatedStorageFileStream configStream = isoStore.OpenFile(preferencesFileName, FileMode.Open))
                {
                    BinaryFormatter binary = new BinaryFormatter();
                    clouds = binary.Deserialize(configStream) as ObservableCollection<Cloud>;
                    this.cloudUrls = binary.Deserialize(configStream) as ObservableCollection<CloudUrl>;
                }
            }
            this.CloudExplorer = new CloudExplorerViewModel(clouds);
            this.CloudExplorer.CloudList.CollectionChanged += CloudsChanged;
            this.cloudUrls.CollectionChanged += CloudUrlsChanged;
            foreach (var cloud in this.CloudExplorer.CloudList)
                cloud.PropertyChanged += CloudChanged;
        }

        private void SavePreferences()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            IsolatedStorageFileStream configStream;
            if (!isoStore.FileExists(preferencesFileName))
                configStream = isoStore.CreateFile(preferencesFileName);
            else
                configStream = isoStore.OpenFile(preferencesFileName, FileMode.Open);
            BinaryFormatter binary = new BinaryFormatter();

            var cloudList = this.CloudExplorer.CloudList.DeepCopy();
            foreach (var cloud in cloudList)
            {
                cloud.ClearServices();
                cloud.ClearApplications();
            }

            binary.Serialize(configStream, cloudList);
            binary.Serialize(configStream, this.cloudUrls);
            configStream.Flush();
            configStream.Close();
        }

        private void RemoveCloud(CloudViewModel cloudView)
        {
            this.Clouds.Remove(cloudView);
        }

        private void OpenApplication(Application application)
        {
            if (application.Parent != null)
                SelectCloud(application.Parent);
            this.SelectedCloudView.SelectedApplication = application;
            this.SelectedCloudView.IsApplicationViewSelected = true;
        }

        private void SelectCloud(Cloud cloud)
        {
            var selectedCloudViewModel = this.Clouds.SingleOrDefault((i) => i.Cloud == cloud);
            if (selectedCloudViewModel == null)
            {
                selectedCloudViewModel = new CloudViewModel(cloud);
                this.Clouds.Add(selectedCloudViewModel);
            }
            this.SelectedCloudView = selectedCloudViewModel;
        }

        private void ProcessCloudListNotification(NotificationMessage<ObservableCollection<Cloud>> message)
        {
            if (message.Notification.Equals(Messages.InitializeClouds))
            {
                var clouds = message.Content;
                this.CloudExplorer = new CloudExplorerViewModel(clouds);
            }
        }

        private void ProcessCloudNotification(NotificationMessage<Cloud> message)
        {
            if (message.Notification.Equals(Messages.OpenCloud))
            {
                var selectedCloudViewModel = this.Clouds.SingleOrDefault((i) => i.Cloud == message.Content);
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

        private void ProcessCloudUrlsNotification(NotificationMessageAction<ObservableCollection<CloudUrl>> message)
        {
            if (message.Notification.Equals(Messages.SetAddCloudData))
                message.Execute(this.cloudUrls);
        }

        public ObservableCollection<CloudViewModel> Clouds
        {
            get { return this.clouds; }
            set { this.clouds = value; RaisePropertyChanged("Clouds"); }
        }

        public CloudExplorerViewModel CloudExplorer
        {
            get { return this.cloudExplorer; }
            set { this.cloudExplorer = value; RaisePropertyChanged("CloudExplorer"); }
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
