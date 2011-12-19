namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using System.Collections.ObjectModel;
    using CloudFoundry.Net.Extensions;
    using CloudFoundry.Net.Types;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight.Messaging;
using System.ComponentModel;

    public class AddApplicationServiceViewModel : DialogViewModel
    {
        private SafeObservableCollection<ProvisionedService> systemServices = new SafeObservableCollection<ProvisionedService>();
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

        protected override void OnConfirmed(CancelEventArgs e)
        {           
            Messenger.Default.Send(new NotificationMessage<bool>(this, true, Messages.AddApplicationServiceDialogResult));
        }

        protected override bool CanExecuteConfirmed()
        {
            return this.SelectedService != null;
        }

        public SafeObservableCollection<ProvisionedService> Services
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
