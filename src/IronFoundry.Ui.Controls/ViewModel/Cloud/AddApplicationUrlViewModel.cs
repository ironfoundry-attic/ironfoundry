using IronFoundry.Ui.Controls.Mvvm;
using IronFoundry.Ui.Controls.Utilities;
using GalaSoft.MvvmLight.Messaging;

namespace IronFoundry.Ui.Controls.ViewModel.Cloud
{
    using Mvvm;
    using Utilities;

    public class AddApplicationUrlViewModel : DialogViewModel
    {
        private string url;

        public AddApplicationUrlViewModel() : base(Messages.AddApplicationUrlDialogResult)
        {
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<AddApplicationUrlViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetAddApplicationUrlData))
                        message.Execute(this);
                    Messenger.Default.Unregister(this);
                });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<string>(Messages.SetAddApplicationUrlData, u => this.url = u));
        }

        public string Url
        {
            get { return this.url; }
            set { this.url = value; RaisePropertyChanged("Url"); }
        }
    }
}