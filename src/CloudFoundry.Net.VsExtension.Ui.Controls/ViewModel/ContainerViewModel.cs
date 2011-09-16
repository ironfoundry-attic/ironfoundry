using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using System.Collections.ObjectModel;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using GalaSoft.MvvmLight.Command;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    [ExportViewModel("Container", true)]
    public class ContainerViewModel : ViewModelBase
    {
        public RelayCommand<CloudViewModel> CloseCloud { get; private set; }

        public ContainerViewModel()
        {
            this.CloseCloud = new RelayCommand<CloudViewModel>(RemoveCloud);
            this.Clouds = new ObservableCollection<CloudViewModel>();
            var sampleData = GetSampleData();
            this.CloudExplorer = new CloudExplorerViewModel(sampleData);
            Messenger.Default.Register<NotificationMessage<Cloud>>(this, AddCloud);
            if (IsInDesignMode)
            {                
                for (int i = 0; i < 3; i++)
                    this.Clouds.Add(new CloudViewModel(sampleData[i]));
            }
        }

        private ObservableCollection<CloudViewModel> clouds;

        public ObservableCollection<CloudViewModel> Clouds
        {
            get { return this.clouds; }
            set
            {
                this.clouds = value;
                RaisePropertyChanged("Clouds"); 
            }
        }

        private CloudExplorerViewModel cloudExplorer;

        public CloudExplorerViewModel CloudExplorer
        {
            get { return this.cloudExplorer; }
            set
            {
                this.cloudExplorer = value;
                RaisePropertyChanged("CloudExplorer");
            }
        }

        private CloudViewModel currentCloudView;
        public CloudViewModel CurrentCloudView
        {
            get { return this.currentCloudView; }
            set
            {
                this.currentCloudView = value;
                RaisePropertyChanged("CurrentCloudView");
            }
        }

        private CloudViewModel selectedCloudView;
        public CloudViewModel SelectedCloudView
        {
            get { return this.selectedCloudView; }
            set
            {
                this.selectedCloudView = value;
                RaisePropertyChanged("SelectedCloudView");
            }
        }

        private void AddCloud(NotificationMessage<Cloud> message)
        {
            if (message.Notification.Equals(Messages.OpenCloud))
            {
                var viewModel = new CloudViewModel(message.Content);
                this.Clouds.Add(viewModel);
                this.SelectedCloudView = viewModel;
            }
        }

        private void RemoveCloud(CloudViewModel cloudView)
        {
            this.Clouds.Remove(cloudView);
        }

        #region Sample Data For DELETE       

        private ObservableCollection<Cloud> GetSampleData()
        {
            return new ObservableCollection<Cloud>(GetRandomObjects<Cloud>(GetSampleCloud));            
        }

        private static Random rnd = new Random();
        private Cloud GetSampleCloud()
        {
            var hostname = string.Format("api.cloud-{0}.org",GetRandomString(5));
            var cloud = new Cloud()
            {                
                ServerName = "Cloud " + rnd.Next(10000),
                Connected = false,
                Email = string.Format("{0}@{1}.com",GetRandomString(15),GetRandomString(10)),
                HostName = hostname,
                Url = string.Format("http://{0}",hostname),
                Password = GetRandomString(10),
                TimeoutStart = rnd.Next(9999),
                TimeoutStop = rnd.Next(9999),
                Applications = GetRandomObjects<Application>(GetSampleApplication)
            };

            return cloud;
        }

        private Application GetSampleApplication()
        {
            var app =  new Application()
            {
                Name = "App " + rnd.Next(10000),
                Cpus = rnd.Next(4),                
                Instances = GetRandomObjects<Instance>(() => new Instance() { Host = GetRandomIP() })                
            };

            return app;
        }

        
        private string GetRandomIP()
        {
            return string.Format("{0}.{1}.{2}.{3}", 192, 168, rnd.Next(255),rnd.Next(255));
        }
        
        private List<T> GetRandomObjects<T>(Func<T> objectCreate)
        {
            var list = new List<T>();
            int count = Convert.ToInt32(rnd.Next(2, 10));
            for (int i = 0; i <= count; i++)
                list.Add(objectCreate());
            return list;
        }

        private string GetRandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * rnd.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString().ToLower();
        }

        #endregion
    }
}
