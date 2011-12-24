namespace IronFoundry.Ui.Controls.ViewModel.Cloud
{
    using GalaSoft.MvvmLight.Messaging;
    using Mvvm;
    using Utilities;

    public class AddApplicationUrlViewModel : DialogViewModel
    {
        private string url;

        public AddApplicationUrlViewModel() : base(Messages.AddApplicationUrlDialogResult)
        {
        }

        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                RaisePropertyChanged("Url");
            }
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<AddApplicationUrlViewModel>>(this,
                                                                                              message =>
                                                                                              {
                                                                                                  if (
                                                                                                      message.
                                                                                                          Notification.
                                                                                                          Equals(
                                                                                                              Messages.
                                                                                                                  GetAddApplicationUrlData))
                                                                                                      message.Execute(
                                                                                                          this);
                                                                                                  Messenger.Default.
                                                                                                      Unregister(this);
                                                                                              });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<string>(Messages.SetAddApplicationUrlData, u => url = u));
        }
    }
}