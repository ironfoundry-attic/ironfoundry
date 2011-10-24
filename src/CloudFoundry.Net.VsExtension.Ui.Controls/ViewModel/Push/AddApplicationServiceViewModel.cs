using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Extensions;
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
    public class AddApplicationServiceViewModel : DialogViewModel
    {
        private ObservableCollection<ProvisionedService> systemServices = new ObservableCollection<ProvisionedService>();
        private ProvisionedService selectedService;

        public AddApplicationServiceViewModel() : base(Messages.AddApplicationServiceDialogResult)
        {            
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<AddApplicationServiceViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetAddApplicationServiceData))
                        message.Execute(this);
                    Cleanup();
                });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Cloud>(Messages.SetAddApplicationServiceData,
                (cloud) => this.Services.Synchronize(cloud.Services, new ProvisionedServiceEqualityComparer())));
        }

        private void Confirmed()
        {           
            Messenger.Default.Send(new NotificationMessage<bool>(this, true, Messages.AddApplicationServiceDialogResult));
        }

        protected override bool CanExecuteConfirmed()
        {
            return this.SelectedService != null;
        }

        public ObservableCollection<ProvisionedService> Services
        {
            get { return this.systemServices; }
        }

        public ProvisionedService SelectedService
        {
            get { return this.selectedService; }
            set { this.selectedService = value; RaisePropertyChanged("SelectedService"); }
        }
    }
}
