using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using IronFoundry.VsExtension.Ui.Controls.Utilities;
using IronFoundry.VsExtension.Ui.Controls.ViewModel;

namespace IronFoundry.VsExtension.Ui.Controls.Views
{
	/// <summary>
	/// Interaction logic for AddCloudUrl.xaml
	/// </summary>
	public partial class AddCloudUrl : Window
	{
		public AddCloudUrl()
		{
			this.InitializeComponent();
            this.DataContext = new AddCloudUrlViewModel();
            this.Closed += (s, e) => Messenger.Default.Unregister(this);

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