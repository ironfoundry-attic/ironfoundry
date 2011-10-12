using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Views
{
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