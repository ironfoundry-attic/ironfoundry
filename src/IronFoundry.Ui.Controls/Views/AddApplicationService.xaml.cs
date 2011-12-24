using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using IronFoundry.Ui.Controls.Utilities;
using IronFoundry.Ui.Controls.ViewModel;
using IronFoundry.Ui.Controls.ViewModel.Push;

namespace IronFoundry.Ui.Controls.Views
{
    using Utilities;
    using ViewModel.Push;

    /// <summary>
    /// Interaction logic for ProvisionService.xaml
	/// </summary>
	public partial class AddApplicationService : Window
	{
        public AddApplicationService()
		{
			this.InitializeComponent();
            this.DataContext = new AddApplicationServiceViewModel();
            this.Closed += (s, e) => Messenger.Default.Unregister(this);

            Messenger.Default.Register<NotificationMessage<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.AddApplicationServiceDialogResult))
                    {
                        this.DialogResult = message.Content;
                        this.Close();
                        Messenger.Default.Unregister(this);
                    }
                });
		}
	}
}