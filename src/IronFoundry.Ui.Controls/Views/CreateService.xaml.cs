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
    /// Interaction logic for ProvisionService.xaml
	/// </summary>
	public partial class CreateService : Window
	{
        public CreateService()
		{
			this.InitializeComponent();
            this.DataContext = new CreateServiceViewModel();
            this.Closed += (s, e) => Messenger.Default.Unregister(this);

            Messenger.Default.Register<NotificationMessage<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.CreateServiceDialogResult))
                    {
                        this.DialogResult = message.Content;
                        this.Close();
                        Messenger.Default.Unregister(this);
                    }
                });
		}
	}
}