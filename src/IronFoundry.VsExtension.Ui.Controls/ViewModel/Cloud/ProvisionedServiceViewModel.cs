namespace IronFoundry.VsExtension.Ui.Controls.ViewModel
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using IronFoundry.Extensions;
    using IronFoundry.Types;
    using IronFoundry.VsExtension.Ui.Controls.Mvvm;
    using IronFoundry.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight.Messaging;

    public class CreateServiceViewModel : DialogViewModel
    {
        private Cloud cloud;
        private string name;
        private readonly SafeObservableCollection<SystemService> systemServices = new SafeObservableCollection<SystemService>();
        private SystemService selectedSystemService;

        public CreateServiceViewModel() : base(Messages.CreateServiceDialogResult)
        {
        }

        protected override void OnConfirmed(CancelEventArgs e)
        {
            var result = provider.CreateService(cloud, SelectedSystemService.Vendor, Name);            
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
                    if (message.Notification.Equals(Messages.GetCreateServiceData))
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
                    this.SystemServices.Synchronize(cloud.AvailableServices, new SystemServiceEqualityComparer());
                }));
        }

        public SafeObservableCollection<SystemService> SystemServices
        {
            get { return this.systemServices; }
        }

        public SystemService SelectedSystemService
        {
            get { return this.selectedSystemService; }
            set { this.selectedSystemService = value; RaisePropertyChanged("SelectedSystemService"); }
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; RaisePropertyChanged("Name"); }
        }
    }
}
