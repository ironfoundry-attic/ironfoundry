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

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class CloudExplorerViewModel : ViewModelBase
    {
        private ObservableCollection<Cloud> cloudList;
        private ObservableCollection<CloudTreeViewItemViewModel> clouds = new ObservableCollection<CloudTreeViewItemViewModel>();        
        public RelayCommand AddCloudCommand { get; private set; }        
        private BackgroundWorker connector = new BackgroundWorker();     

        public CloudExplorerViewModel(ObservableCollection<Cloud> cloudList)
        {            
            this.CloudList = cloudList;                        
            AddCloudCommand = new RelayCommand(AddCloud);

            connector.DoWork += new DoWorkEventHandler(BeginConnect);
            connector.RunWorkerCompleted += new RunWorkerCompletedEventHandler(EndConnect);
            connector.RunWorkerAsync(cloudList);               
        }

        public ObservableCollection<Cloud> CloudList
        {
            get { return this.cloudList; }
            set
            {
                this.cloudList = value;
                foreach (var cloud in cloudList)
                {
                    var cloudView = clouds.SingleOrDefault((i) => i.Cloud.ID == cloud.ID);
                    if (cloudView != null)
                        cloudView.Cloud = cloud;
                    else
                        clouds.Add(new CloudTreeViewItemViewModel(cloud));
                }                   
            }
        }

        public ObservableCollection<CloudTreeViewItemViewModel> Clouds
        {
            get { return this.clouds; }
        }

        private void AddCloud()
        {
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.AddCloud,
                (confirmed) =>
                {
                    if (confirmed)
                    {
                        Messenger.Default.Send(new NotificationMessageAction<AddCloudViewModel>(Messages.GetAddCloudData,
                            (viewModel) =>
                            {
                                CloudList.Add(viewModel.Cloud);
                                clouds.Add(new CloudTreeViewItemViewModel(viewModel.Cloud));
                            }));
                    }
                }));
        }

        private void BeginConnect(object sender, DoWorkEventArgs args)
        {
            var cloudList = args.Argument as ObservableCollection<Cloud>;
            var manager = new VcapClient();
            Dictionary<Cloud, List<Application>> dictionary = new Dictionary<Cloud, List<Application>>();            
            foreach (var cloud in cloudList)
            {
                VcapClientResult result = manager.Login(cloud);
                if (result.Success)
                {
                    Cloud serverCloud = result.Cloud;
                    var applications = manager.ListApps(serverCloud);
                    dictionary.Add(serverCloud, applications);
                }
            }
            args.Result = dictionary;
        }

        private void EndConnect(object sender, RunWorkerCompletedEventArgs args)
        {
            var dictionary = args.Result as Dictionary<Cloud,List<Application>>;
            foreach (var item in dictionary)
            {
                var cloud = this.CloudList.Single((i) => item.Key.ID == i.ID);
                foreach (var application in item.Value)
                {
                    var current = cloud.Applications.SingleOrDefault((i) => i.Name == application.Name);
                    if (current != null)
                        current = application;
                    else
                        cloud.Applications.Add(application);
                }                
            }
            CommandManager.InvalidateRequerySuggested();
        }        
    }
}
