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
            //this.CloudExplorer = new CloudExplorerViewModel(new ObservableCollection<Cloud>());
            this.CloudExplorer = new CloudExplorerViewModel(sampleData);
            this.cloudUrls = GetBaseCloudUrls();
            Messenger.Default.Register<NotificationMessage<Cloud>>(this, ProcessCloudNotification);
            Messenger.Default.Register<NotificationMessage<Application>>(this, ProcessApplicationNotification);
            Messenger.Default.Register<NotificationMessageAction<ObservableCollection<CloudUrl>>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.SetAddCloudData))
                        message.Execute(this.cloudUrls);
                });

            if (IsInDesignMode)
            {                
                for (int i = 0; i < 3; i++)
                    this.Clouds.Add(new CloudViewModel(sampleData[i]));
            }
        }

        private ObservableCollection<CloudUrl> cloudUrls;

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
            } else if (message.Notification.Equals(Messages.StopApplication))
            {
                this.SelectedCloudView.Stop();
            } else if (message.Notification.Equals(Messages.RestartApplication))
            {
                this.SelectedCloudView.Restart();
            }
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
        

        private void RemoveCloud(CloudViewModel cloudView)
        {
            this.Clouds.Remove(cloudView);
        }

        #region Sample Data For DELETE       

        private ObservableCollection<CloudUrl> GetBaseCloudUrls()
        {
            ObservableCollection<CloudUrl> cloudUrls = new ObservableCollection<CloudUrl>() {
                new CloudUrl() { ServerType = "Local cloud", Url = "http://api.vcap.me", IsConfigurable = false},
                new CloudUrl() { ServerType = "Microcloud", Url = "http://api.{mycloud}.cloudfoundry.me", IsConfigurable = true, IsMicroCloud = true },
                new CloudUrl() { ServerType = "VMware Cloud Foundry", Url = "https://api.cloudfoundry.com", IsDefault = true },
                new CloudUrl() { ServerType = "vmforce", Url = "http://api.alpha.vmforce.com", IsConfigurable = false}
            };
            return cloudUrls;
        }

        private ObservableCollection<Cloud> GetSampleData()
        {
            return new ObservableCollection<Cloud>(GetRandomObjects<Cloud,object>(null,GetSampleCloud));            
        }

        private static Random rnd = new Random();
        private Cloud GetSampleCloud(int i, object parent)
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
            };

            cloud.Applications = GetRandomObjects<Application, Cloud>(cloud, GetSampleApplication);
            cloud.Services = GetRandomObjects<Service, Cloud>(cloud, GetSampleService);

            return cloud;
        }

        private Application GetSampleApplication(int i, Cloud parent)
        {
            var app =  new Application()
            {
                Name = "App " + rnd.Next(10000),
                Cpus = rnd.Next(4),
                State = CloudFoundry.Net.Types.Instance.InstanceState.STOPPED,
                MappedUrls = GetRandomUrls(),
                MemoryLimit = CloudFoundry.Net.VsExtension.Ui.Controls.Model.Constants.MemoryLimits[rnd.Next(0,5)],
                Parent = parent                
            };

            app.Instances = GetRandomObjects<Instance,Application>(app,GetSampleInstance);
            app.InstanceCount = app.Instances.Count;
            app.Services = GetRandomObjects<Service, Cloud>(parent, GetSampleService);            

            return app;
        }

        private ObservableCollection<string> GetRandomUrls()
        {
            string[] com = { "com", "org", "ca", "net", "uk" };
            ObservableCollection<string> retString = new ObservableCollection<string>();
            for (int i = 2; i < rnd.Next(4, 7); i++)
                retString.Add(GetRandomString(12) + "." + com[rnd.Next(4)]);
            return retString;
        }

        private Service GetSampleService(int i, Cloud parent)
        {
            string [] serviceTypes = { "Database", "Web Service", "Service Type 1", "Service Type 2"};
            string [] vendors = { "MySql", "Postgres", "Sql Server", "Sybase"};
            string [] versions = { "4.0", "5.1", "10.0", "15.3.1"};
            var service = new Service()
            {
                Name = "Service " + rnd.Next(10000),
                ServiceType = serviceTypes[rnd.Next(0, 3)],
                Vendor = vendors[rnd.Next(0, 3)],
                Version = versions[rnd.Next(0, 3)],
                Parent = parent
            };
            return service;
        }

        private Instance GetSampleInstance(int i, Application parent)
        {
            var instance = new Instance()
            {
                CpuPercent = Convert.ToDecimal(rnd.NextDouble() * 100.00),
                Disk = rnd.Next(0, 2048),
                Host = GetRandomIP(),
                ID = i,
                Memory = CloudFoundry.Net.VsExtension.Ui.Controls.Model.Constants.MemoryLimits[rnd.Next(0, 5)],
                Parent = parent,
                Uptime = DateTime.Now - DateTime.Now.Subtract(new TimeSpan((long)rnd.Next(200000)))
            };
            return instance;
        }

        
        private string GetRandomIP()
        {
            return string.Format("{0}.{1}.{2}.{3}", 192, 168, rnd.Next(255),rnd.Next(255));
        }

        private ObservableCollection<T> GetRandomObjects<T, U>(U parent, Func<int, U, T> objectCreate)
        {
            var list = new ObservableCollection<T>();
            int count = Convert.ToInt32(rnd.Next(2, 10));
            for (int i = 0; i <= count; i++)
                list.Add(objectCreate(i,parent));
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
