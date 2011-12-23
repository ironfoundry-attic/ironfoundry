using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using IronFoundry.Ui.Controls.Utilities;
using IronFoundry.Ui.Controls.ViewModel;
using IronFoundry.Ui.Controls.ViewModel.AddCloud;

namespace IronFoundry.Ui.Controls.Views
{
    using Utilities;
    using ViewModel.AddCloud;

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