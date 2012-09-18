namespace IronFoundry.Ui.Controls.ViewModel.Cloud
{
    using System.ComponentModel;
    using Extensions;
    using GalaSoft.MvvmLight.Messaging;
    using Model;
    using Models;
    using Mvvm;
    using Utilities;

    public class CreateServiceViewModel : DialogViewModel
    {
        private readonly SafeObservableCollection<SystemService> systemServices =
            new SafeObservableCollection<SystemService>();

        private Cloud cloud;
        private string name;

        private SystemService selectedSystemService;

        public CreateServiceViewModel() : base(Messages.CreateServiceDialogResult)
        {
        }

        public SafeObservableCollection<SystemService> SystemServices
        {
            get { return systemServices; }
        }

        public SystemService SelectedSystemService
        {
            get { return selectedSystemService; }
            set
            {
                selectedSystemService = value;
                RaisePropertyChanged("SelectedSystemService");
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        protected override void OnConfirmed(CancelEventArgs e)
        {
            ProviderResponse<bool> result = provider.CreateService(cloud, SelectedSystemService.Vendor, Name);
            if (!result.Response)
            {
                ErrorMessage = result.Message;
                e.Cancel = true;
            }
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<CreateServiceViewModel>>(this,
                                                                                          message =>
                                                                                          {
                                                                                              if (
                                                                                                  message.Notification.
                                                                                                      Equals(
                                                                                                          Messages.
                                                                                                              GetCreateServiceData))
                                                                                                  message.Execute(this);
                                                                                              Cleanup();
                                                                                          });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Cloud>(Messages.SetCreateServiceData,
                                                                        (cloud) =>
                                                                        {
                                                                            this.cloud = cloud;
                                                                            SystemServices.Synchronize(
                                                                                cloud.AvailableServices,
                                                                                new SystemServiceEqualityComparer());
                                                                        }));
        }
    }
}