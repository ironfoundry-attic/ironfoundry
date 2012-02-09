namespace IronFoundry.Ui.Controls.Views
{
    using System.Windows;
    using GalaSoft.MvvmLight.Messaging;
    using IronFoundry.Ui.Controls.Utilities;
    using IronFoundry.Ui.Controls.ViewModel;

    public partial class ManageClouds : Window
    {
        private readonly ManageCloudsViewModel viewModel = new ManageCloudsViewModel();

        public ManageClouds()
        {
            InitializeComponent();

            this.DataContext = viewModel;

            Messenger.Default.Register<NotificationMessage<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.PreferencesDialogResult))
                    {
                        this.DialogResult = message.Content;
                        this.Close();
                        Messenger.Default.Unregister(this);
                    }
                });
        }

        private void AddCloud_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddCloud();
        }

        private void RemoveCloud_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RemoveSelectedCloud();
        }
    }
}