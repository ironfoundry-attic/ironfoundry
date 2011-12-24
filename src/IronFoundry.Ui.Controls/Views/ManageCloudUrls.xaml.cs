namespace IronFoundry.Ui.Controls.Views
{
    using System.Windows;
    using GalaSoft.MvvmLight.Messaging;
    using Utilities;
    using ViewModel.AddCloud;

    /// <summary>
    /// Interaction logic for ManageCloudUrls.xaml
    /// </summary>
    public partial class ManageCloudUrls : Window
    {
        public ManageCloudUrls()
        {
            this.InitializeComponent();
            this.DataContext = new ManageCloudUrlsViewModel();
            this.Closed += (s, e) => Messenger.Default.Unregister(this);

            Messenger.Default.Register<NotificationMessage<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ManageCloudUrlsDialogResult))
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
                    if (message.Notification.Equals(Messages.AddCloudUrl))
                    {
                        var view = new Views.AddCloudUrl();
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
                   if (message.Notification.Equals(Messages.CreateMicrocloudTarget))
                   {
                       var view = new Views.CreateMicrocloudTarget();
                       Window parentWindow = Window.GetWindow(this);
                       view.Owner = parentWindow;
                       var result = view.ShowDialog();
                       message.Execute(result.GetValueOrDefault());
                   }
               });
        }
    }
}
