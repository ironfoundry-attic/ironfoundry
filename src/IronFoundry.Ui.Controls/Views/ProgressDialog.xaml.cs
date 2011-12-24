namespace IronFoundry.Ui.Controls.Views
{
    using System.Windows;
    using GalaSoft.MvvmLight.Messaging;
    using Utilities;
    using ViewModel;

    public partial class ProgressDialog : Window
    {
        public ProgressDialog()
        {            
            InitializeComponent();
            this.DataContext = new ProgressViewModel();
            this.Closed += (s, e) => Messenger.Default.Unregister(this);

            Messenger.Default.Register<NotificationMessage<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ProgressDialogResult))
                    {
                        this.DialogResult = message.Content;
                        this.Close();
                        Messenger.Default.Unregister(this);
                    }
                });
        }
    }
}