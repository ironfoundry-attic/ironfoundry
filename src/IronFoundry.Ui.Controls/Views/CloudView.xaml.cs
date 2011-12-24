using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using IronFoundry.Ui.Controls.Utilities;

namespace IronFoundry.Ui.Controls.Views
{
    using Utilities;

    /// <summary>
    /// Interaction logic for FoundryProperties.xaml
    /// </summary>
    public partial class CloudView : UserControl
    {
        public CloudView()
        {
            InitializeComponent();            
            this.Unloaded += (s, e) => Messenger.Default.Unregister(this);

            Messenger.Default.Register<NotificationMessageAction<bool>>(
                this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ChangePassword))
                    {
                        var view = new Views.ChangePassword();
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
                    if (message.Notification.Equals(Messages.ManageApplicationUrls))
                    {
                        var view = new Views.ManageApplicationUrls();
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
                    if (message.Notification.Equals(Messages.CreateService))
                    {
                        var view = new Views.CreateService();
                        Window parentWindow = Window.GetWindow(this);
                        view.Owner = parentWindow;
                        var result = view.ShowDialog();
                        message.Execute(result.GetValueOrDefault());
                    }
                });            
        }
    }
}
