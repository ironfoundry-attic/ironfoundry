using IronFoundry.Types;
using IronFoundry.Ui.Controls.Mvvm;
using IronFoundry.Ui.Controls.Utilities;
using GalaSoft.MvvmLight.Messaging;

namespace IronFoundry.Ui.Controls.ViewModel.AddCloud
{
    using Mvvm;
    using Utilities;

    public class AddCloudUrlViewModel : DialogViewModel
    {
        private CloudUrl cloudUrl = new CloudUrl();

        public AddCloudUrlViewModel()
            : base(Messages.AddCloudUrlDialogResult)
        {
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<CloudUrl>(Messages.SetAddCloudUrlData,
                (cloudUrl) =>
                {
                    this.cloudUrl = cloudUrl;
                }));
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<AddCloudUrlViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetAddCloudUrlData))
                        message.Execute(this);
                    Cleanup();
                });
        }

        public string Name
        {
            get { return this.cloudUrl.ServerType; }
            set { this.cloudUrl.ServerType = value; RaisePropertyChanged("Name"); }
        }

        public string Url
        {
            get { return this.cloudUrl.Url; }
            set { this.cloudUrl.Url = value; RaisePropertyChanged("Url"); }
        }
    }
}