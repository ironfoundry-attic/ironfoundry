using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Views
{
	/// <summary>
	/// Interaction logic for CreateMicrocloudTarget.xaml
	/// </summary>
	public partial class CreateMicrocloudTarget : Window
	{
		public CreateMicrocloudTarget()
		{
			this.InitializeComponent();
            Messenger.Default.Register<NotificationMessage<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.CreateMicrocloudTargetDialogResult))
                    {
                        this.DialogResult = message.Content;
                        this.Close();
                        Messenger.Default.Unregister(this);
                    }
                });
		}
	}
}