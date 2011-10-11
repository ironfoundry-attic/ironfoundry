using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using CloudFoundry.Net.Types;
using System.Collections.ObjectModel;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    [ExportViewModel("ProvisionService", false)]
    public class ProvisionedServiceViewModel : ViewModelBase
    {
        public RelayCommand ConfirmedCommand { get; private set; }
        public RelayCommand CancelledCommand { get; private set; }
        string name;
        private ObservableCollection<SystemService> systemServices = new ObservableCollection<SystemService>();
        private SystemService selectedSystemService;

        public ProvisionedServiceViewModel()
        {
            ConfirmedCommand = new RelayCommand(Confirmed);
            CancelledCommand = new RelayCommand(Cancelled);

            InitializeData();
            RegisterGetData();
        }

        private void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<ProvisionedServiceViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetProvisionServiceData))
                        message.Execute(this);
                    Messenger.Default.Unregister(this);
                });
        }

        private void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Cloud>(Messages.SetProvisionServiceData,
                (cloud) =>
                {
                    this.SystemServices.Synchronize(cloud.AvailableServices, new SystemServiceEqualityComparer());
                }));
        }

        private void Confirmed()
        {           
            Messenger.Default.Send(new NotificationMessage<bool>(this, true, Messages.ProvisionServiceDialogResult));
        }

        private void Cancelled()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, false, Messages.ProvisionServiceDialogResult));
            Messenger.Default.Unregister(this);
        }

        public ObservableCollection<SystemService> SystemServices
        {
            get { return this.systemServices; }
        }

        public SystemService SelectedSystemService
        {
            get { return this.selectedSystemService; }
            set
            {
                this.selectedSystemService = value;
                RaisePropertyChanged("SelectedSystemService");
            }
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                RaisePropertyChanged("Name");
            }
        }
    }
}
