namespace IronFoundry.Ui.Controls.ViewModel.AddCloud
{
    using GalaSoft.MvvmLight.Messaging;
    using Mvvm;
    using Types;
    using Utilities;

    public class AddCloudUrlViewModel : DialogViewModel
    {
        private CloudUrl cloudUrl = new CloudUrl();

        public AddCloudUrlViewModel()
            : base(Messages.AddCloudUrlDialogResult)
        {
        }

        public string Name
        {
            get { return cloudUrl.ServerName; }
            set
            {
                cloudUrl.ServerName = value;
                RaisePropertyChanged("Name");
            }
        }

        public string Url
        {
            get { return cloudUrl.Url; }
            set
            {
                cloudUrl.Url = value;
                RaisePropertyChanged("Url");
            }
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<CloudUrl>(Messages.SetAddCloudUrlData,
                                                                           (cloudUrl) => { this.cloudUrl = cloudUrl; }));
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<AddCloudUrlViewModel>>(this,
                                                                                        message =>
                                                                                        {
                                                                                            if (
                                                                                                message.Notification.
                                                                                                    Equals(
                                                                                                        Messages.
                                                                                                            GetAddCloudUrlData))
                                                                                                message.Execute(this);
                                                                                            Cleanup();
                                                                                        });
        }
    }
}