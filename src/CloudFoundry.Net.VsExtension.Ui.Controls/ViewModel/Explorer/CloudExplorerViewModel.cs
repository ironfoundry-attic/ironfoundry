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

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    [ExportViewModel("CloudExplorer", true)]
    public class CloudExplorerViewModel : ViewModelBase
    {
        private CloudFoundryProvider provider;
        private ObservableCollection<CloudTreeViewItemViewModel> clouds = new ObservableCollection<CloudTreeViewItemViewModel>();        
        public RelayCommand AddCloudCommand { get; private set; }        
        private BackgroundWorker connector = new BackgroundWorker();

        public CloudExplorerViewModel()
        {
            Messenger.Default.Send<NotificationMessageAction<CloudFoundryProvider>>(new NotificationMessageAction<CloudFoundryProvider>(Messages.GetCloudFoundryProvider, LoadProvider));            
            AddCloudCommand = new RelayCommand(AddCloud);
        }

        private void LoadProvider(CloudFoundryProvider provider)
        {
            this.provider = provider;
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
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.AddCloud,(confirmed) => {}));                
        }              
    }
}
