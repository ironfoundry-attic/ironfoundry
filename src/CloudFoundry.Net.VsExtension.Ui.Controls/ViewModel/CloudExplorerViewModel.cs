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
        private ObservableCollection<Cloud> cloudList;

        public CloudExplorerViewModel(ObservableCollection<Cloud> cloudList)
        {
            this.cloudList = cloudList;            
            this.clouds = new ObservableCollection<CloudTreeViewItemViewModel>(
                (from cloud in cloudList
                 select new CloudTreeViewItemViewModel(cloud)).ToList());

            AddCloudCommand = new RelayCommand(AddCloud);
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
                                this.cloudList.Add(viewModel.Cloud);
                                this.clouds.Add(new CloudTreeViewItemViewModel(viewModel.Cloud));
                            }));
                    }
                }));
        }
    }
}
