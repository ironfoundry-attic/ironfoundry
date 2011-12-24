namespace IronFoundry.Ui.Controls.Views
{
    using System.Windows;
    using GalaSoft.MvvmLight.Messaging;
    using Utilities;
    using ViewModel.Cloud;

    /// <summary>
    /// Interaction logic for ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : Window
    {
        public ChangePassword()
        {
            InitializeComponent();
            this.DataContext = new ChangePasswordViewModel();
            this.Closed += (s, e) => Messenger.Default.Unregister(this);

            Messenger.Default.Register<NotificationMessage<bool>>(this,               
                message  =>
                {
                    if (message.Notification.Equals(Messages.ChangePasswordDialogResult))
                    {
                        this.DialogResult = message.Content;
                        this.Close();
                        Messenger.Default.Unregister(this);
                    }
                });
        }
    }
}