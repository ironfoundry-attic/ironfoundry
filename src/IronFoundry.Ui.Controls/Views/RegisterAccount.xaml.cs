namespace IronFoundry.Ui.Controls.Views
{
    using System.Windows;
    using GalaSoft.MvvmLight.Messaging;
    using Utilities;
    using ViewModel.AddCloud;

    /// <summary>
    /// Interaction logic for ChangePassword.xaml
    /// </summary>
    public partial class RegisterAccount : Window
    {
        public RegisterAccount()
        {
            InitializeComponent();
            this.DataContext = new RegisterAccountViewModel();
            this.Closed += (s, e) => Messenger.Default.Unregister(this);

            Messenger.Default.Register<NotificationMessage<bool>>(this,               
                message  =>
                {
                    if (message.Notification.Equals(Messages.RegisterAccountDialogResult))
                    {
                        this.DialogResult = message.Content;
                        this.Close();
                        Messenger.Default.Unregister(this);
                    }
                });
        }
    }
}