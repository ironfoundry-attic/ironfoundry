using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Views
{
	/// <summary>
	/// Interaction logic for AddCloudUrl.xaml
	/// </summary>
	public partial class AddCloudUrl : Window
	{
		public AddCloudUrl()
		{
			this.InitializeComponent();

            Messenger.Default.Register<NotificationMessage<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.AddCloudUrlDialogResult))
                    {
                        this.DialogResult = message.Content;
                        this.Close();
                        Messenger.Default.Unregister(this);
                    }
                });
		}
	}
}