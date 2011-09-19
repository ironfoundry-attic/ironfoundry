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

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class CloudExplorerViewModel : ViewModelBase
    {      
        private ObservableCollection<CloudTreeViewItemViewModel> clouds;
        public RelayCommand AddCloudCommand { get; private set; }

        public CloudExplorerViewModel(ObservableCollection<Cloud> cloudList)
        {
            AddCloudCommand = new RelayCommand(AddCloud);
            this.clouds = new ObservableCollection<CloudTreeViewItemViewModel>(
                (from cloud in cloudList
                 select new CloudTreeViewItemViewModel(cloud)).ToList());
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

                    }
                }));
        }
    }
}
