namespace IronFoundry.VsExtension.Ui.Controls.Views
{
    using System.Windows;
    using IronFoundry.VsExtension.Ui.Controls.Utilities;
    using IronFoundry.VsExtension.Ui.Controls.ViewModel;
    using GalaSoft.MvvmLight.Messaging;

    /// <summary>
    /// Interaction logic for AddCloud.xaml
    /// </summary>
    public partial class AddCloud : Window
    {
        public AddCloud()
        {
            InitializeComponent();
            this.DataContext = new AddCloudViewModel();
            this.Closed += (s, e) => Messenger.Default.Unregister(this);

            Messenger.Default.Register<NotificationMessage<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.AddCloudDialogResult))
                    {
                        this.DialogResult = message.Content;
                        this.Close();
                        Messenger.Default.Unregister(this);
                    }
                });

            Messenger.Default.Register<NotificationMessageAction<bool>>(
                this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ManageCloudUrls))
                    {
                        var view = new Views.ManageCloudUrls();
                        Window parentWindow = Window.GetWindow(this);
                        view.Owner = parentWindow;
                        var result = view.ShowDialog();
                        message.Execute(result.GetValueOrDefault());
                    }
                });

            Messenger.Default.Register<NotificationMessageAction<bool>>(
               this,
               message =>
               {
                   if (message.Notification.Equals(Messages.RegisterAccount))
                   {
                       var view = new Views.RegisterAccount();
                       Window parentWindow = Window.GetWindow(this);
                       view.Owner = parentWindow;
                       var result = view.ShowDialog();
                       message.Execute(result.GetValueOrDefault());
                   }
               });
        }
    }
}
