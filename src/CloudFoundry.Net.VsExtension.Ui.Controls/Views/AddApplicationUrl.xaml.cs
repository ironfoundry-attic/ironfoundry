using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Views
{
	/// <summary>
    /// Interaction logic for AddApplicationUrl.xaml
	/// </summary>
	public partial class AddApplicationUrl : Window
	{
        public AddApplicationUrl()
		{
			this.InitializeComponent();

            Messenger.Default.Register<NotificationMessage<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.AddApplicationUrlDialogResult))
                    {
                        this.DialogResult = message.Content;
                        this.Close();
                        Messenger.Default.Unregister(this);
                    }
                });
		}
	}
}