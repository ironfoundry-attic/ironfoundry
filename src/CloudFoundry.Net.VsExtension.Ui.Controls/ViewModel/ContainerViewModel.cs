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


        public ContainerViewModel()
        {
            InitializeClouds();
            CloseCloud = new RelayCommand<CloudViewModel>(RemoveCloud);
            Clouds = new ObservableCollection<CloudViewModel>();
            cloudUrls = GetBaseCloudUrls();

            Messenger.Default.Register<NotificationMessage<ObservableCollection<Cloud>>>(this, ProcessCloudListNotification);
            Messenger.Default.Register<NotificationMessage<Cloud>>(this, ProcessCloudNotification);
            Messenger.Default.Register<NotificationMessage<Application>>(this, ProcessApplicationNotification);
            Messenger.Default.Register<NotificationMessageAction<ObservableCollection<CloudUrl>>>(this, ProcessCloudUrlsNotification);
        }

        private void InitializeClouds()
        {
            try
            {
                ObservableCollection<Cloud> clouds = new ObservableCollection<Cloud>();
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
                if (isoStore.FileExists("clouds.bin"))
                {
                    using (IsolatedStorageFileStream configStream = isoStore.OpenFile("clouds.bin", FileMode.Open))
                    {
                        BinaryFormatter binary = new BinaryFormatter();
                        clouds = binary.Deserialize(configStream) as ObservableCollection<Cloud>;
                    }
                }
                this.CloudExplorer = new CloudExplorerViewModel(clouds);
                this.CloudExplorer.CloudList.CollectionChanged += CloudsChanged;
                foreach (var cloud in this.CloudExplorer.CloudList)
                    cloud.PropertyChanged += CloudChanged;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
                    SaveClouds();
                    break;
                default:
                    break;
            }
        }

        public void CloudsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SaveClouds();
        }

        private void SaveClouds()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            IsolatedStorageFileStream configStream;
            if (!isoStore.FileExists("clouds.bin"))
                configStream = isoStore.CreateFile("clouds.bin");
            else
                configStream = isoStore.OpenFile("clouds.bin", FileMode.Open);
            BinaryFormatter binary = new BinaryFormatter();

            ObservableCollection<Cloud> weakClouds = new ObservableCollection<Cloud>();
            foreach (var cloud in this.CloudExplorer.CloudList)
            {
                weakClouds.Add(new Cloud()
                {
                    AccessToken = cloud.AccessToken,
                    Email = cloud.Email,
                    ServerName = cloud.ServerName,
                    HostName = cloud.HostName,
                    Url = cloud.Url,
                    TimeoutStart = cloud.TimeoutStart,
                    TimeoutStop = cloud.TimeoutStop,
                    ID = cloud.ID,
                    Password = cloud.Password,
                    IsConnected = cloud.IsConnected,
                    IsDisconnected = cloud.IsDisconnected
                });
            }

            binary.Serialize(configStream, weakClouds);
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

        private static ObservableCollection<CloudUrl> GetBaseCloudUrls()
        {
            ObservableCollection<CloudUrl> cloudUrls = new ObservableCollection<CloudUrl>() {
                new CloudUrl() { ServerType = "Local cloud", Url = "http://api.vcap.me", IsConfigurable = false},
                new CloudUrl() { ServerType = "Microcloud", Url = "http://api.{mycloud}.cloudfoundry.me", IsConfigurable = true, IsMicroCloud = true },
                new CloudUrl() { ServerType = "VMware Cloud Foundry", Url = "https://api.cloudfoundry.com", IsDefault = true },
                new CloudUrl() { ServerType = "vmforce", Url = "http://api.alpha.vmforce.com", IsConfigurable = false}
            };
            return cloudUrls;
        }

        public RelayCommand<CloudViewModel> CloseCloud { get; private set; }

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
