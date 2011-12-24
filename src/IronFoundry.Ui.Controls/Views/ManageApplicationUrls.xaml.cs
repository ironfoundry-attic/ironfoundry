using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using IronFoundry.Ui.Controls.Utilities;
using IronFoundry.Ui.Controls.ViewModel;
using IronFoundry.Ui.Controls.ViewModel.Cloud;

namespace IronFoundry.Ui.Controls.Views
{
    using Utilities;
    using ViewModel.Cloud;

    /// <summary>
	/// Interaction logic for ManageCloudUrls.xaml
	/// </summary>
	public partial class ManageApplicationUrls : Window
	{
        public ManageApplicationUrls()
		{
			this.InitializeComponent();
            this.DataContext = new ManageApplicationUrlsViewModel();
            this.Closed += (s, e) => Messenger.Default.Unregister(this);

            Messenger.Default.Register<NotificationMessage<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ManageApplicationUrlsDialogResult))
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
                    if (message.Notification.Equals(Messages.AddApplicationUrl))
                    {
                        var view = new Views.AddApplicationUrl();
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