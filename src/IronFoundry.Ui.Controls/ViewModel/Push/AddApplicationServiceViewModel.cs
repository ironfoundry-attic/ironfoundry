namespace IronFoundry.Ui.Controls.ViewModel.Push
{
    using System.ComponentModel;
    using Extensions;
    using GalaSoft.MvvmLight.Messaging;
    using Models;
    using Mvvm;
    using Utilities;

    public class AddApplicationServiceViewModel : DialogViewModel
    {
        private readonly SafeObservableCollection<ProvisionedService> systemServices =
            new SafeObservableCollection<ProvisionedService>();

        private ProvisionedService selectedService;

        public AddApplicationServiceViewModel() : base(Messages.AddApplicationServiceDialogResult)
        {
        }

        public SafeObservableCollection<ProvisionedService> Services
        {
            get { return systemServices; }
        }

        public ProvisionedService SelectedService
        {
            get { return selectedService; }
            set
            {
                selectedService = value;
                RaisePropertyChanged("SelectedService");
            }
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<AddApplicationServiceViewModel>>(this,
                                                                                                  message =>
                                                                                                  {
                                                                                                      if (
                                                                                                          message.
                                                                                                              Notification
                                                                                                              .Equals(
                                                                                                                  Messages
                                                                                                                      .
                                                                                                                      GetAddApplicationServiceData))
                                                                                                          message.
                                                                                                              Execute(
                                                                                                                  this);
                                                                                                      Cleanup();
                                                                                                  });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Cloud>(Messages.SetAddApplicationServiceData,
                                                                        (cloud) =>
                                                                        Services.Synchronize(cloud.Services,
                                                                                             new ProvisionedServiceEqualityComparer
                                                                                                 ())));
        }

        protected override void OnConfirmed(CancelEventArgs e)
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, true, Messages.AddApplicationServiceDialogResult));
        }

        protected override bool CanExecuteConfirmed()
        {
            return SelectedService != null;
        }
    }
}